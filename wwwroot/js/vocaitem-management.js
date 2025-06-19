// Đối tượng quản lý từ vựng - Version Fixed
const VocaItemManager = {
    vocaSetId: null,

    // Khởi tạo
    init: function (vocaSetId) {
        this.vocaSetId = vocaSetId;
        this.setupEventHandlers();
    },

    // Thiết lập các sự kiện
    setupEventHandlers: function () {
        $('#addVocaItemBtn').on('click', () => {
            VocaItemManager.showAddItemModal();
        });

        $('#importVocaItemBtn').on('click', () => {
            $('#importModal').removeClass('hidden');
        });

        $('#vocaItemForm').on('submit', function (e) {
            e.preventDefault();
            VocaItemManager.saveVocaItem();
        });

        $('#closeModalBtn, #cancelModalBtn').on('click', () => {
            $('#vocaItemModal').addClass('hidden');
        });

        $('#closeImportModalBtn, #cancelImportBtn').on('click', () => {
            $('#importModal').addClass('hidden');
        });

        $('#processImportBtn').on('click', () => {
            VocaItemManager.processImport();
        });

        $('#searchVocaItem').on('input', () => {
            VocaItemManager.filterItems();
        });

        $('#filterStatus').on('change', () => {
            VocaItemManager.filterItems();
        });
    },

    // Hiển thị modal thêm từ mới
    showAddItemModal: function () {
        $('#vocaItemForm')[0].reset();
        $('#vocaItemId').val('');
        $('#modalTitle').text('Thêm từ vựng mới');
        $('#vocaItemModal').removeClass('hidden');
    },

    // Tải danh sách từ vựng
    loadVocaItems: function () {
        console.log('Loading VocaItems for VocaSetId:', this.vocaSetId);

        $.ajax({
            url: $('#loadItemsUrl').val() || '/VocaItem/GetItemsByVocaSetId',
            type: 'GET',
            dataType: 'json',
            data: { vocaSetId: this.vocaSetId },
            success: function (response) {
                console.log('Load VocaItems response:', response);

                if (response && response.success) {
                    VocaItemManager.renderVocaItems(response.data);
                    VocaItemManager.updateStatistics(response.data);
                } else {
                    const errorMsg = response && response.message ? response.message : 'Không thể tải danh sách từ vựng';
                    console.error('Load items failed:', response);
                    alert('Lỗi: ' + errorMsg);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error loading items:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });

                let errorMessage = 'Đã xảy ra lỗi khi tải danh sách từ vựng.';
                if (xhr.status === 404) {
                    errorMessage = 'Không tìm thấy trang tải từ vựng. Vui lòng kiểm tra đường dẫn.';
                } else if (xhr.status === 401 || xhr.status === 403) {
                    errorMessage = 'Bạn không có quyền truy cập. Vui lòng đăng nhập lại.';
                } else if (xhr.status === 500) {
                    errorMessage = 'Lỗi máy chủ. Vui lòng thử lại sau.';
                }
                alert(errorMessage);
            }
        });
    },

    // Hiển thị danh sách từ vựng
    renderVocaItems: function (items) {
        const container = $('#vocaItemsList');
        container.empty();

        if (!items || items.length === 0) {
            container.html('<div class="text-center py-4 text-gray-500">Chưa có từ vựng nào. Hãy thêm từ mới!</div>');
            return;
        }

        console.log('Rendering', items.length, 'items');

        items.forEach(function (item, index) {
            console.log('Rendering item', index, ':', item);

            const itemHtml = `
                <div class="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition vocabulary-item" data-id="${item.id}">
                    <div class="flex justify-between items-start">
                        <div class="flex-1">
                            <h4 class="font-semibold text-lg text-gray-800">${item.word || 'N/A'}</h4>
                            <p class="text-sm text-gray-600 mt-1">${item.meaning || 'Chưa có nghĩa'}</p>
                            ${item.wordType ? `<span class="inline-block bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full mt-2">${item.wordType}</span>` : ''}
                            ${item.pronunciation ? `<p class="text-sm text-gray-500 mt-1 italic">${item.pronunciation}</p>` : ''}
                        </div>
                        <div class="flex space-x-3 ml-4">
                            <button type="button" class="edit-item-btn bg-blue-500 hover:bg-blue-600 text-white px-3 py-2 rounded-md transition-colors duration-200 flex items-center space-x-1" data-id="${item.id}" title="Chỉnh sửa từ vựng">
                                <i class="fas fa-edit"></i>
                                <span class="hidden sm:inline text-xs">Sửa</span>
                            </button>
                            <button type="button" class="delete-item-btn bg-red-500 hover:bg-red-600 text-white px-3 py-2 rounded-md transition-colors duration-200 flex items-center space-x-1" data-id="${item.id}" title="Xóa từ vựng">
                                <i class="fas fa-trash"></i>
                                <span class="hidden sm:inline text-xs">Xóa</span>
                            </button>
                        </div>
                    </div>
                    ${item.exampleSentence ? `<div class="mt-3 p-2 bg-gray-50 rounded text-sm text-gray-700"><strong>Ví dụ:</strong> ${item.exampleSentence}</div>` : ''}
                </div>
            `;
            container.append(itemHtml);
        });

        // Gắn sự kiện cho các nút edit và delete
        container.off('click', '.edit-item-btn').on('click', '.edit-item-btn', function (e) {
            e.preventDefault();
            e.stopPropagation();
            const itemId = $(this).data('id');
            console.log('Edit button clicked for item:', itemId);
            VocaItemManager.editItem(itemId);
        });

        container.off('click', '.delete-item-btn').on('click', '.delete-item-btn', function (e) {
            e.preventDefault();
            e.stopPropagation();
            const itemId = $(this).data('id');
            console.log('Delete button clicked for item:', itemId);
            VocaItemManager.deleteItem(itemId);
        });

        console.log('Rendered items successfully. Buttons attached.');
    },

    // Cập nhật thống kê
    updateStatistics: function (items) {
        if (!items) return;

        $('#totalWordsCount').text(items.length);
        $('#learnedWordsCount').text(items.filter(i => i.status === 'learned').length);
        $('#unlearnedWordsCount').text(items.filter(i => i.status === 'notlearned').length);
    },

    // Lưu từ vựng - Function đã được fix hoàn toàn
    // Hàm saveVocaItem cải tiến với error handling tốt hơn
    saveVocaItem: function () {
        console.log('Starting saveVocaItem function');

        // Validate required fields
        const word = $('#word').val()?.trim();
        if (!word) {
            alert('Lỗi: Vui lòng nhập từ vựng.');
            $('#word').focus();
            return;
        }

        const itemId = $('#vocaItemId').val();
        const isNew = !itemId || itemId === '';

        console.log('Item ID:', itemId, 'Is new:', isNew);

        const url = isNew ?
            ($('#addItemUrl').val() || '/VocaItem/AddItem') :
            ($('#updateItemUrl').val() || '/VocaItem/UpdateItem');

        console.log('Using URL:', url);

        const token = $('#vocaItemForm').find('input[name="__RequestVerificationToken"]').val();
        if (!token) {
            alert('Lỗi: Không tìm thấy CSRF token. Vui lòng tải lại trang.');
            return;
        }

        // Thu thập dữ liệu form với validation
        const formData = {
            __RequestVerificationToken: token,
            VocaSetId: parseInt(VocaItemManager.vocaSetId) || 0,
            Word: word,
            WordType: $('#wordType').val()?.trim() || '',
            Pronunciation: $('#pronunciation').val()?.trim() || '',
            AudioUrl: $('#audioUrl').val()?.trim() || '',
            Meaning: $('#meaning').val()?.trim() || '',
            ExampleSentence: $('#exampleSentence').val()?.trim() || ''
        };

        // Thêm ID cho trường hợp chỉnh sửa
        if (!isNew) {
            formData.Id = parseInt(itemId) || 0;
            if (formData.Id <= 0) {
                alert('Lỗi: ID từ vựng không hợp lệ.');
                return;
            }
        }

        console.log('Sending formData:', formData);

        // Disable button để tránh double submit
        const submitBtn = $('#vocaItemForm').find('button[type="submit"]');
        const originalText = submitBtn.text();
        submitBtn.prop('disabled', true).text('Đang lưu...');

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            dataType: 'json',
            timeout: 30000, // 30 second timeout
            success: function (response) {
                console.log('Save response:', response);

                if (response && response.success) {
                    $('#vocaItemModal').addClass('hidden');
                    VocaItemManager.loadVocaItems();

                    const message = response.message || (isNew ? 'Thêm từ vựng thành công!' : 'Cập nhật từ vựng thành công!');
                    alert('Thành công: ' + message);
                } else {
                    let errorMsg = 'Không thể lưu từ vựng';

                    if (response && response.message) {
                        errorMsg = response.message;
                    }

                    // Hiển thị chi tiết lỗi validation nếu có
                    if (response && response.errors) {
                        const errorDetails = Object.entries(response.errors)
                            .map(([field, errors]) => `${field}: ${Array.isArray(errors) ? errors.join(', ') : errors}`)
                            .join('\n');
                        errorMsg += '\n\nChi tiết lỗi:\n' + errorDetails;
                    }

                    alert('Lỗi: ' + errorMsg);
                    console.error('Save item failed:', response);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error saving item:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });

                let errorMessage = 'Đã xảy ra lỗi khi lưu từ vựng.';

                if (status === 'timeout') {
                    errorMessage = 'Yêu cầu bị timeout. Vui lòng thử lại.';
                } else if (xhr.status === 400) {
                    try {
                        const errorResponse = JSON.parse(xhr.responseText);
                        errorMessage = errorResponse.message || 'Dữ liệu không hợp lệ.';
                        if (errorResponse.errors) {
                            const errorDetails = Object.entries(errorResponse.errors)
                                .map(([field, errors]) => `${field}: ${Array.isArray(errors) ? errors.join(', ') : errors}`)
                                .join('\n');
                            errorMessage += '\n\nChi tiết lỗi:\n' + errorDetails;
                        }
                    } catch (e) {
                        errorMessage = 'Dữ liệu không hợp lệ hoặc định dạng phản hồi không mong muốn.';
                    }
                } else if (xhr.status === 403) {
                    errorMessage = 'Lỗi xác thực CSRF (token không hợp lệ). Vui lòng tải lại trang.';
                } else if (xhr.status === 404) {
                    errorMessage = 'Không tìm thấy endpoint API. Vui lòng kiểm tra cấu hình.';
                } else if (xhr.status === 500) {
                    errorMessage = 'Lỗi máy chủ. Vui lòng thử lại sau.';
                }

                alert('Lỗi: ' + errorMessage);
            },
            complete: function () {
                // Re-enable button
                submitBtn.prop('disabled', false).text(originalText);
            }
        });
    },


    // Sửa từ vựng
    editItem: function (itemId) {
        console.log('Editing item with ID:', itemId);

        $.ajax({
            url: $('#getItemUrl').val() || '/VocaItem/GetItemById',
            type: 'GET',
            dataType: 'json',
            data: { id: itemId },
            success: function (response) {
                if (response && response.success && response.data) {
                    const item = response.data;
                    $('#vocaItemId').val(item.id);
                    $('#word').val(item.word || '');
                    $('#wordType').val(item.wordType || '');
                    $('#pronunciation').val(item.pronunciation || '');
                    $('#audioUrl').val(item.audioUrl || '');
                    $('#meaning').val(item.meaning || '');
                    $('#exampleSentence').val(item.exampleSentence || '');
                    $('#modalTitle').text('Chỉnh sửa từ vựng');
                    $('#vocaItemModal').removeClass('hidden');
                } else {
                    const errorMsg = response && response.message ? response.message : 'Không thể tải thông tin từ vựng';
                    alert('Lỗi: ' + errorMsg);
                    console.error('Get item failed:', response);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error getting item:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });
                alert('Đã xảy ra lỗi khi tải thông tin từ vựng.');
            }
        });
    },

    // Xóa từ vựng
    deleteItem: function (itemId) {
        console.log('Deleting item with ID:', itemId);

        if (!confirm('Bạn có chắc chắn muốn xóa từ vựng này không?')) {
            return;
        }

        const token = $('#vocaItemForm').find('input[name="__RequestVerificationToken"]').val();

        if (!token) {
            alert('Lỗi: Không tìm thấy CSRF token. Vui lòng tải lại trang.');
            return;
        }

        $.ajax({
            url: $('#deleteItemUrl').val() || '/VocaItem/DeleteItem',
            type: 'POST',
            data: {
                id: itemId,
                __RequestVerificationToken: token
            },
            dataType: 'json',
            success: function (response) {
                if (response && response.success) {
                    VocaItemManager.loadVocaItems();
                    const message = response.message || 'Đã xóa từ vựng thành công!';
                    alert('Thành công: ' + message);
                } else {
                    const errorMsg = response && response.message ? response.message : 'Không thể xóa từ vựng';
                    alert('Lỗi: ' + errorMsg);
                    console.error('Delete item failed:', response);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error deleting item:', {
                    status: xhr.status,
                    statusText: xhr.responseText,
                    error: error
                });
                let errorMessage = 'Đã xảy ra lỗi khi xóa từ vựng.';
                if (xhr.status === 403) {
                    errorMessage = 'Lỗi xác thực CSRF. Vui lòng tải lại trang.';
                } else if (xhr.status === 500) {
                    errorMessage = 'Lỗi máy chủ. Vui lòng thử lại sau.';
                }

                alert('Lỗi: ' + errorMessage);
            }
        });
    },

    // Xử lý import từ vựng
    processImport: function () {
        const importData = $('#importData').val();
        if (!importData.trim()) {
            alert('Vui lòng nhập dữ liệu import.');
            return;
        }

        const lines = importData.split('\n');
        const items = [];

        lines.forEach(function (line) {
            if (!line.trim()) return;
            const parts = line.split(',');
            if (parts.length < 2) return;
            items.push({
                VocaSetId: VocaItemManager.vocaSetId,
                Word: parts[0].trim(),
                WordType: parts.length > 1 ? parts[1].trim() : '',
                Pronunciation: parts.length > 2 ? parts[2].trim() : '',
                Meaning: parts.length > 3 ? parts[3].trim() : '',
                ExampleSentence: parts.length > 4 ? parts[4].trim() : ''
            });
        });

        if (items.length === 0) {
            alert('Không có dữ liệu hợp lệ để import.');
            return;
        }

        const token = $('#vocaItemForm').find('input[name="__RequestVerificationToken"]').val();

        if (!token) {
            alert('Lỗi: Không tìm thấy CSRF token. Vui lòng tải lại trang.');
            return;
        }

        const payload = {
            VocaSetId: VocaItemManager.vocaSetId,
            Items: items,
            __RequestVerificationToken: token
        };

        $.ajax({
            url: $('#importItemsUrl').val() || '/VocaItem/ImportItems',
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(payload),
            beforeSend: function (xhr) {
                xhr.setRequestHeader('RequestVerificationToken', token);
            },
            success: function (response) {
                if (response && response.success) {
                    $('#importModal').addClass('hidden');
                    $('#importData').val('');
                    VocaItemManager.loadVocaItems();
                    const message = response.message ||
                        `Đã import ${response.data?.imported || 0} từ vựng thành công. ${response.data?.duplicates || 0} từ bị bỏ qua do trùng lặp.`;
                    alert('Thành công: ' + message);
                } else {
                    const errorMsg = response && response.message ? response.message : 'Không thể import từ vựng';
                    alert('Lỗi: ' + errorMsg);
                    console.error('Import failed:', response);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error importing items:', {
                    status: xhr.status,
                    statusText: xhr.responseText,
                    error: error
                });

                let errorMessage = 'Đã xảy ra lỗi khi import từ vựng.';
                if (xhr.status === 403) {
                    errorMessage = 'Lỗi xác thực CSRF. Vui lòng tải lại trang.';
                } else if (xhr.status === 500) {
                    errorMessage = 'Lỗi máy chủ. Vui lòng thử lại sau.';
                }

                alert('Lỗi: ' + errorMessage);
            }
        });
    },

    // Lọc danh sách từ vựng
    filterItems: function () {
        const keyword = $('#searchVocaItem').val().toLowerCase();
        const status = $('#filterStatus').val();

        $.ajax({
            url: $('#loadItemsUrl').val() || '/VocaItem/GetItemsByVocaSetId',
            type: 'GET',
            dataType: 'json',
            data: {
                vocaSetId: this.vocaSetId,
                keyword: keyword,
                status: status
            },
            success: function (response) {
                if (response && response.success) {
                    VocaItemManager.renderVocaItems(response.data);
                } else {
                    const errorMsg = response && response.message ? response.message : 'Không thể lọc danh sách từ vựng';
                    alert('Lỗi: ' + errorMsg);
                    console.error('Filter failed:', response);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error filtering items:', {
                    status: xhr.status,
                    statusText: xhr.responseText,
                    error: error
                });
                alert('Đã xảy ra lỗi khi lọc danh sách từ vựng.');
            }
        });
    }
};