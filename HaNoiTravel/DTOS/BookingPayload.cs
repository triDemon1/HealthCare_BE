using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class BookingPayload
    {
        // Payload for creating a new booking
        [Required]
        public int customerId { get; set; }

        [Required]
        public int addressId { get; set; }

        [Required]
        public int serviceId { get; set; }

        // Subject details: either an existing SubjectId or data for a new subject
        public int? subjectId { get; set; } // Optional: ID of an existing subject
        public NewSubjectData? NewSubjectData { get; set; } // Optional: Data for a new subject

        public int? staffId { get; set; } // Optional: Assigned staff

        public decimal priceAtBooking { get; set; } // Price at the time of booking

        [Required]
        public DateTime scheduledStartTime { get; set; }
        public DateTime scheduledEndTime { get; set; }

        public string? notes { get; set; }

        // Add validation logic here or use data annotations
        // Example: Custom validation to ensure either SubjectId or NewSubjectData is provided
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (subjectId == null && NewSubjectData == null)
            {
                yield return new ValidationResult(
                    "Either SubjectId or NewSubjectData must be provided.",
                    new[] { nameof(subjectId), nameof(NewSubjectData) });
            }
            if (subjectId != null && NewSubjectData != null)
            {
                yield return new ValidationResult(
                   "Cannot provide both SubjectId and NewSubjectData.",
                   new[] { nameof(subjectId), nameof(NewSubjectData) });
            }
        }
    }
    public class NewSubjectData
    {
        [Required]
        public int typeId { get; set; } // Subject Type (e.g., Elderly, Child, Pet)

        [MaxLength(150)]
        public string? name { get; set; }

        public DateOnly? dateOfBirth { get; set; }

        public bool? gender { get; set; } // true for Male, false for Female

        public string? medicalNotes { get; set; }

        [MaxLength(2048)]
        public string? imageUrl { get; set; }
    }
}
