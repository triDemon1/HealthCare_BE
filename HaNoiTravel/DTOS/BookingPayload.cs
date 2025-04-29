using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class BookingPayload
    {
        // Payload for creating a new booking
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        // Subject details: either an existing SubjectId or data for a new subject
        public int? SubjectId { get; set; } // Optional: ID of an existing subject
        public NewSubjectData? NewSubjectData { get; set; } // Optional: Data for a new subject

        public int? StaffId { get; set; } // Optional: Assigned staff

        [Required]
        public decimal PriceAtBooking { get; set; } // Price at the time of booking

        [Required]
        public DateTime ScheduledStartTime { get; set; }

        [Required]
        public DateTime ScheduledEndTime { get; set; }

        public string? Notes { get; set; }

        // Add validation logic here or use data annotations
        // Example: Custom validation to ensure either SubjectId or NewSubjectData is provided
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SubjectId == null && NewSubjectData == null)
            {
                yield return new ValidationResult(
                    "Either SubjectId or NewSubjectData must be provided.",
                    new[] { nameof(SubjectId), nameof(NewSubjectData) });
            }
            if (SubjectId != null && NewSubjectData != null)
            {
                yield return new ValidationResult(
                   "Cannot provide both SubjectId and NewSubjectData.",
                   new[] { nameof(SubjectId), nameof(NewSubjectData) });
            }
        }
    }
    public class NewSubjectData
    {
        [Required]
        public int TypeId { get; set; } // Subject Type (e.g., Elderly, Child, Pet)

        [MaxLength(150)]
        public string? Name { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public bool? Gender { get; set; } // true for Male, false for Female

        public string? MedicalNotes { get; set; }

        [MaxLength(2048)]
        public string? ImageUrl { get; set; }
    }
}
