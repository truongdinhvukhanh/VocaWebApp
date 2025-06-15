using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Implementation của IVocaSetRepository
    /// Sử dụng Entity Framework Core để thao tác với database
    /// </summary>
    public class VocaSetRepository : IVocaSetRepository
    {
        private readonly ApplicationDbContext _context;

        public VocaSetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region CRUD Operations

        /// <summary>
        /// Lấy VocaSet theo ID (không bao gồm deleted)
        /// </summary>
        public async Task<VocaSet?> GetByIdAsync(int id)
        {
            return await _context.VocaSets
                .Where(v => v.Id == id && !v.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy VocaSet theo ID bao gồm navigation properties
        /// </summary>
        public async Task<VocaSet?> GetByIdWithIncludesAsync(int id, params string[] includeProperties)
        {
            var query = _context.VocaSets.Where(v => v.Id == id && !v.IsDeleted);

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy tất cả VocaSet không bị xóa và không bị ẩn
        /// </summary>
        public async Task<IEnumerable<VocaSet>> GetAllAsync()
        {
            return await _context.VocaSets
                .Where(v => !v.IsDeleted && !v.IsHidden)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Thêm VocaSet mới vào database
        /// </summary>
        public async Task<VocaSet> AddAsync(VocaSet vocaSet)
        {
            // Thiết lập các giá trị mặc định
            vocaSet.CreatedAt = DateTime.UtcNow;
            vocaSet.ViewCount = 0;
            vocaSet.IsDeleted = false;
            vocaSet.IsHidden = false;

            if (string.IsNullOrEmpty(vocaSet.Status))
            {
                vocaSet.Status = "private";
            }

            _context.VocaSets.Add(vocaSet);
            await _context.SaveChangesAsync();
            return vocaSet;
        }

        /// <summary>
        /// Cập nhật thông tin VocaSet
        /// </summary>
        public async Task<VocaSet> UpdateAsync(VocaSet vocaSet)
        {
            var existingVocaSet = await _context.VocaSets.FindAsync(vocaSet.Id);
            if (existingVocaSet == null)
            {
                throw new ArgumentException("VocaSet không tồn tại", nameof(vocaSet));
            }

            // Cập nhật các thuộc tính có thể thay đổi
            existingVocaSet.Name = vocaSet.Name;
            existingVocaSet.Description = vocaSet.Description;
            existingVocaSet.Keywords = vocaSet.Keywords;
            existingVocaSet.Status = vocaSet.Status;
            existingVocaSet.FolderId = vocaSet.FolderId;

            await _context.SaveChangesAsync();
            return existingVocaSet;
        }

        /// <summary>
        /// Xóa mềm VocaSet (đặt IsDeleted = true)
        /// </summary>
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;

            vocaSet.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xóa cứng VocaSet khỏi database (chỉ dành cho admin)
        /// </summary>
        public async Task<bool> HardDeleteAsync(int id)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;

            _context.VocaSets.Remove(vocaSet);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region User-specific Operations

        /// <summary>
        /// Lấy tất cả VocaSet của user
        /// </summary>
        public async Task<IEnumerable<VocaSet>> GetByUserIdAsync(string userId, bool includeDeleted = false)
        {
            var query = _context.VocaSets.Where(v => v.UserId == userId);

            if (!includeDeleted)
            {
                query = query.Where(v => !v.IsDeleted);
            }

            return await query
                .OrderByDescending(v => v.LastAccessed ?? v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaSet theo user và folder
        /// </summary>
        public async Task<IEnumerable<VocaSet>> GetByUserAndFolderAsync(string userId, int? folderId)
        {
            return await _context.VocaSets
                .Where(v => v.UserId == userId && v.FolderId == folderId && !v.IsDeleted)
                .OrderByDescending(v => v.LastAccessed ?? v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaSet được truy cập gần đây nhất của user
        /// </summary>
        public async Task<IEnumerable<VocaSet>> GetRecentlyAccessedAsync(string userId, int limit = 10)
        {
            return await _context.VocaSets
                .Where(v => v.UserId == userId && !v.IsDeleted && v.LastAccessed.HasValue)
                .OrderByDescending(v => v.LastAccessed)
                .Take(limit)
                .ToListAsync();
        }

        #endregion

        #region Search and Filter Operations

        /// <summary>
        /// Tìm kiếm VocaSet theo từ khóa trong tên, mô tả và keywords
        /// </summary>
        public async Task<IEnumerable<VocaSet>> SearchAsync(string keyword, string? userId = null, bool searchInPublic = true)
        {
            var query = _context.VocaSets.Where(v => !v.IsDeleted && !v.IsHidden);

            // Tìm kiếm theo từ khóa
            query = query.Where(v =>
                v.Name.Contains(keyword) ||
                (v.Description != null && v.Description.Contains(keyword)) ||
                (v.Keywords != null && v.Keywords.Contains(keyword)));

            // Lọc theo user hoặc public
            if (userId != null)
            {
                if (searchInPublic)
                {
                    query = query.Where(v => v.UserId == userId ||
                        (v.Status == "public-view" || v.Status == "public-copy"));
                }
                else
                {
                    query = query.Where(v => v.UserId == userId);
                }
            }
            else if (searchInPublic)
            {
                query = query.Where(v => v.Status == "public-view" || v.Status == "public-copy");
            }

            return await query
                .OrderByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Tìm VocaSet theo điều kiện tùy chỉnh
        /// </summary>
        public async Task<IEnumerable<VocaSet>> FindAsync(Expression<Func<VocaSet, bool>> predicate)
        {
            return await _context.VocaSets
                .Where(predicate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaSet public có thể xem (trạng thái public-view hoặc public-copy)
        /// </summary>
        public async Task<IEnumerable<VocaSet>> GetPublicViewableAsync(string? excludeUserId = null)
        {
            var query = _context.VocaSets
                .Where(v => !v.IsDeleted && !v.IsHidden &&
                    (v.Status == "public-view" || v.Status == "public-copy"));

            if (excludeUserId != null)
            {
                query = query.Where(v => v.UserId != excludeUserId);
            }

            return await query
                .OrderByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaSet public có thể sao chép (trạng thái public-copy)
        /// </summary>
        public async Task<IEnumerable<VocaSet>> GetPublicCopyableAsync(string? excludeUserId = null)
        {
            var query = _context.VocaSets
                .Where(v => !v.IsDeleted && !v.IsHidden && v.Status == "public-copy");

            if (excludeUserId != null)
            {
                query = query.Where(v => v.UserId != excludeUserId);
            }

            return await query
                .OrderByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Status and Visibility Management

        /// <summary>
        /// Cập nhật trạng thái chia sẻ của VocaSet
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;

            // Validate status
            var validStatuses = new[] { "private", "public-view", "public-copy" };
            if (!validStatuses.Contains(status))
            {
                throw new ArgumentException("Trạng thái không hợp lệ", nameof(status));
            }

            vocaSet.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Cập nhật trạng thái ẩn/hiện VocaSet (dành cho admin)
        /// </summary>
        public async Task<bool> UpdateVisibilityAsync(int id, bool isHidden)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;

            vocaSet.IsHidden = isHidden;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Khôi phục VocaSet đã bị xóa mềm
        /// </summary>
        public async Task<bool> RestoreAsync(int id)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;

            vocaSet.IsDeleted = false;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Statistics and Analytics

        /// <summary>
        /// Tăng số lượt xem VocaSet
        /// </summary>
        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;

            vocaSet.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Cập nhật thời gian truy cập cuối cùng
        /// </summary>
        public async Task<bool> UpdateLastAccessedAsync(int id, DateTime? accessTime = null)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;

            vocaSet.LastAccessed = accessTime ?? DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy thống kê VocaSet của user
        /// </summary>
        public async Task<VocaSetStatistics> GetUserStatisticsAsync(string userId)
        {
            var userVocaSets = await _context.VocaSets
                .Where(v => v.UserId == userId && !v.IsDeleted)
                .ToListAsync();

            var totalWords = await _context.VocaItems
                .Where(vi => userVocaSets.Select(vs => vs.Id).Contains(vi.VocaSetId))
                .CountAsync();

            return new VocaSetStatistics
            {
                TotalVocaSets = userVocaSets.Count,
                PublicVocaSets = userVocaSets.Count(v => v.Status != "private"),
                PrivateVocaSets = userVocaSets.Count(v => v.Status == "private"),
                TotalWords = totalWords,
                TotalViews = userVocaSets.Sum(v => v.ViewCount),
                LastCreated = userVocaSets.OrderByDescending(v => v.CreatedAt).FirstOrDefault()?.CreatedAt,
                LastAccessed = userVocaSets.Where(v => v.LastAccessed.HasValue)
                    .OrderByDescending(v => v.LastAccessed).FirstOrDefault()?.LastAccessed
            };
        }

        /// <summary>
        /// Lấy VocaSet phổ biến nhất theo lượt xem
        /// </summary>
        public async Task<IEnumerable<VocaSet>> GetMostPopularAsync(int limit = 10, int? timeFrame = null)
        {
            var query = _context.VocaSets.Where(v => !v.IsDeleted && !v.IsHidden &&
                (v.Status == "public-view" || v.Status == "public-copy"));

            if (timeFrame.HasValue)
            {
                var fromDate = DateTime.UtcNow.AddDays(-timeFrame.Value);
                query = query.Where(v => v.CreatedAt >= fromDate);
            }

            return await query
                .OrderByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        #endregion

        #region Copy Operations

        /// <summary>
        /// Sao chép VocaSet public sang tài khoản của user
        /// </summary>
        public async Task<VocaSet?> CopyVocaSetAsync(int originalSetId, string userId, string? newName = null, int? folderId = null)
        {
            // Kiểm tra quyền sao chép
            if (!await CanCopyAsync(originalSetId, userId))
            {
                return null;
            }

            var originalSet = await _context.VocaSets
                .Include(v => v.VocaItems)
                .FirstOrDefaultAsync(v => v.Id == originalSetId);

            if (originalSet == null) return null;

            // Tạo VocaSet mới
            var newVocaSet = new VocaSet
            {
                UserId = userId,
                FolderId = folderId,
                Name = newName ?? $"{originalSet.Name} (Copy)",
                Description = originalSet.Description,
                Keywords = originalSet.Keywords,
                Status = "private", // Mặc định là private cho bản sao
                CreatedAt = DateTime.UtcNow,
                ViewCount = 0,
                IsDeleted = false,
                IsHidden = false
            };

            _context.VocaSets.Add(newVocaSet);
            await _context.SaveChangesAsync();

            // Sao chép các VocaItem
            if (originalSet.VocaItems?.Any() == true)
            {
                var newVocaItems = originalSet.VocaItems.Select(vi => new VocaItem
                {
                    VocaSetId = newVocaSet.Id,
                    Word = vi.Word,
                    WordType = vi.WordType,
                    Pronunciation = vi.Pronunciation,
                    AudioUrl = vi.AudioUrl,
                    Meaning = vi.Meaning,
                    ExampleSentence = vi.ExampleSentence,
                    Status = "notlearned" // Reset trạng thái học
                }).ToList();

                _context.VocaItems.AddRange(newVocaItems);
                await _context.SaveChangesAsync();
            }

            // Ghi lại lịch sử copy
            var copyRecord = new VocaSetCopy
            {
                OriginalSetId = originalSetId,
                CopiedByUserId = userId,
                CopiedAt = DateTime.UtcNow
            };

            _context.VocaSetCopies.Add(copyRecord);
            await _context.SaveChangesAsync();

            return newVocaSet;
        }

        /// <summary>
        /// Kiểm tra VocaSet có thể được sao chép bởi user không
        /// </summary>
        public async Task<bool> CanCopyAsync(int id, string userId)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null || vocaSet.IsDeleted || vocaSet.IsHidden)
                return false;

            // Không thể copy VocaSet của chính mình
            if (vocaSet.UserId == userId)
                return false;

            // Chỉ copy được VocaSet có trạng thái public-copy
            return vocaSet.Status == "public-copy";
        }

        #endregion

        #region Pagination and Sorting

        /// <summary>
        /// Lấy VocaSet với phân trang và sắp xếp
        /// </summary>
        public async Task<PagedResult<VocaSet>> GetPagedAsync(int pageNumber, int pageSize, string? userId = null,
            string sortBy = "created", bool sortDescending = true)
        {
            var query = _context.VocaSets.Where(v => !v.IsDeleted && !v.IsHidden);

            // Lọc theo user
            if (userId != null)
            {
                query = query.Where(v => v.UserId == userId);
            }
            else
            {
                // Chỉ lấy public nếu không chỉ định user
                query = query.Where(v => v.Status == "public-view" || v.Status == "public-copy");
            }

            // Sắp xếp
            query = sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name),
                "created" => sortDescending ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt),
                "accessed" => sortDescending ? query.OrderByDescending(v => v.LastAccessed) : query.OrderBy(v => v.LastAccessed),
                "viewcount" => sortDescending ? query.OrderByDescending(v => v.ViewCount) : query.OrderBy(v => v.ViewCount),
                _ => sortDescending ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<VocaSet>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Đếm số lượng VocaSet theo điều kiện
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<VocaSet, bool>>? predicate = null)
        {
            var query = _context.VocaSets.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
        }

        #endregion

        #region Validation and Business Rules

        /// <summary>
        /// Kiểm tra user có quyền truy cập VocaSet không
        /// </summary>
        public async Task<bool> HasAccessAsync(int vocaSetId, string userId)
        {
            var vocaSet = await _context.VocaSets.FindAsync(vocaSetId);
            if (vocaSet == null || vocaSet.IsDeleted || vocaSet.IsHidden)
                return false;

            // Owner luôn có quyền truy cập
            if (vocaSet.UserId == userId)
                return true;

            // Public VocaSet có thể truy cập
            return vocaSet.Status == "public-view" || vocaSet.Status == "public-copy";
        }

        /// <summary>
        /// Kiểm tra user có quyền chỉnh sửa VocaSet không
        /// </summary>
        public async Task<bool> HasEditPermissionAsync(int vocaSetId, string userId)
        {
            var vocaSet = await _context.VocaSets.FindAsync(vocaSetId);
            if (vocaSet == null || vocaSet.IsDeleted)
                return false;

            // Chỉ owner mới có quyền chỉnh sửa
            return vocaSet.UserId == userId;
        }

        /// <summary>
        /// Kiểm tra tên VocaSet có trùng lặp trong tài khoản user không
        /// </summary>
        public async Task<bool> IsNameExistsAsync(string name, string userId, int? excludeId = null)
        {
            var query = _context.VocaSets
                .Where(v => v.UserId == userId && v.Name == name && !v.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        #endregion
    }
}
