using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model đại diện cho một từ vựng trong bộ từ vựng
    /// Chứa tất cả thông tin chi tiết về từ vựng
    /// </summary>
    public class VocaItem
    {
        /// <summary>
        /// ID duy nhất của từ vựng
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của bộ từ vựng chứa từ này
        /// Foreign key tới VocaSet
        /// Bắt buộc - mỗi từ phải thuộc về một bộ từ vựng
        /// </summary>
        [Required]
        [ValidateNever]
        public int VocaSetId { get; set; }

        /// <summary>
        /// Navigation property tới bộ từ vựng chứa
        /// Dùng để truy cập thông tin VocaSet từ VocaItem
        /// </summary>
        [ValidateNever]
        [JsonIgnore]
        public VocaSet VocaSet { get; set; }

        /// <summary>
        /// Từ vựng chính
        /// Bắt buộc nhập, tối đa 100 ký tự
        /// Nội dung chính của flashcard
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Word { get; set; }

        /// <summary>
        /// Từ loại của từ vựng
        /// Tối đa 50 ký tự
        /// Ví dụ: noun, verb, adjective, adverb...
        /// </summary>
        [MaxLength(50)]
        public string? WordType { get; set; }

        /// <summary>
        /// Phiên âm của từ vựng
        /// Tối đa 100 ký tự
        /// Ví dụ: /həˈloʊ/ cho từ "hello"
        /// </summary>
        [MaxLength(100)]
        public string? Pronunciation { get; set; }

        /// <summary>
        /// URL file âm thanh phát âm
        /// Tối đa 255 ký tự để lưu đường dẫn
        /// Link tới file mp3/wav để phát âm
        /// </summary>
        [MaxLength(255)]
        public string? AudioUrl { get; set; }

        /// <summary>
        /// Nghĩa của từ vựng
        /// Tối đa 255 ký tự
        /// Định nghĩa hoặc bản dịch của từ
        /// </summary>
        [MaxLength(255)]
        public string? Meaning { get; set; }

        /// <summary>
        /// Câu ví dụ sử dụng từ vựng
        /// Không giới hạn độ dài
        /// Giúp user hiểu cách sử dụng từ trong ngữ cảnh
        /// </summary>
        public string? ExampleSentence { get; set; }

        /// <summary>
        /// Trạng thái học tập của từ vựng
        /// Tối đa 20 ký tự
        /// Mặc định "not_learned", có thể là "learned", "reviewing"
        /// Dùng để tracking progress của user
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "not_learned";

        /// <summary>
        /// Lịch sử học tập của từ vựng này
        /// Quan hệ một-nhiều: một từ có nhiều lần học
        /// Dùng để tracking performance và spaced repetition
        /// </summary>
        [ValidateNever]
        public ICollection<VocaItemHistory> Histories { get; set; }
    }
}