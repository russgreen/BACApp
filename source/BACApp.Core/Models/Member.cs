using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class Member
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("post_code")]
    public string? PostCode { get; set; }

    [JsonPropertyName("user_type")]
    public string? UserType { get; set; }

    // Dates are provided as strings in the API
    [JsonPropertyName("medical_expiry_date")]
    public string? MedicalExpiryDate { get; set; }

    [JsonPropertyName("medical_certificate_type")]
    public string? MedicalCertificateType { get; set; }

    [JsonPropertyName("sep_expiry_date")]
    public string? SepExpiryDate { get; set; }

    [JsonPropertyName("mep_expiry_date")]
    public string? MepExpiryDate { get; set; }

    [JsonPropertyName("ir_se_expiry_date")]
    public string? IrSeExpiryDate { get; set; }

    [JsonPropertyName("night_rating")]
    public int? NightRating { get; set; }

    // Schema says integer but example is string-like token; accept string for resilience
    [JsonPropertyName("forgot_password_token")]
    public string? ForgotPasswordToken { get; set; }

    // number for boolean flags (1/0)
    [JsonPropertyName("active")]
    public int? Active { get; set; }

    [JsonPropertyName("initials")]
    public string? Initials { get; set; }

    [JsonPropertyName("next_of_kin_name")]
    public string? NextOfKinName { get; set; }

    [JsonPropertyName("next_of_kin_number")]
    public string? NextOfKinNumber { get; set; }

    [JsonPropertyName("pilot_licence_number")]
    public string? PilotLicenceNumber { get; set; }

    [JsonPropertyName("fi_expiry_date")]
    public string? FiExpiryDate { get; set; }

    [JsonPropertyName("exams_validity_expiry_date")]
    public string? ExamsValidityExpiryDate { get; set; }

    [JsonPropertyName("fi_restricted")]
    public int? FiRestricted { get; set; }

    [JsonPropertyName("exam_portal_registration")]
    public int? ExamPortalRegistration { get; set; }

    [JsonPropertyName("ir_me_expiry_date")]
    public string? IrMeExpiryDate { get; set; }

    [JsonPropertyName("ir_r_expiry_date")]
    public string? IrRExpiryDate { get; set; }

    [JsonPropertyName("lapl")]
    public int? Lapl { get; set; }

    [JsonPropertyName("pilot_licence_type")]
    public string? PilotLicenceType { get; set; }

    [JsonPropertyName("aerobatic_rating")]
    public int? AerobaticRating { get; set; }

    [JsonPropertyName("display_authorization_expiry_date")]
    public string? DisplayAuthorizationExpiryDate { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}
