using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    public interface IFolderRepository
    {
        /// <summary>
        /// Lấy folder theo ID và UserID (kiểm tra quyền sở hữu)
        /// </summary>
        Task<Folder?> GetByIdAsync(int folderId, string userId);

        /// <summary>
        /// Lấy tất cả folder của người dùng + phân trang + sắp xếp
        /// </summary>
        Task<IEnumerable<Folder>> GetByUserAsync(string userId,
            string? sortBy = "CreatedAt",
            bool ascending = false,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Tìm kiếm folder theo tên/mô tả (dùng cho chức năng search trong trang Folder)
        /// </summary>
        Task<IEnumerable<Folder>> SearchAsync(string userId, string keyword);

        /// <summary>
        /// Tạo mới folder (kiểm tra trùng tên trong cùng user)
        /// </summary>
        Task<Folder> AddAsync(Folder folder);

        /// <summary>
        /// Cập nhật thông tin folder (name, description)
        /// </summary>
        Task UpdateAsync(Folder folder);

        /// <summary>
        /// Xóa folder (kiểm tra có VocaSet nào không trước khi xóa)
        /// </summary>
        Task DeleteAsync(int folderId, string userId);

        /// <summary>
        /// Kiểm tra tên folder đã tồn tại trong user chưa
        /// </summary>
        Task<bool> CheckNameExistsAsync(string userId, string folderName);

        /// <summary>
        /// Lấy folder cùng với danh sách VocaSet bên trong (cho trang hiển thị chi tiết folder)
        /// </summary>
        Task<Folder?> GetWithVocaSetsAsync(int folderId, string userId);
    }

}
