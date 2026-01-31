using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class Invoice
{
	[JsonPropertyName("invoice_id")]
	public int InvoiceId { get; set; }

	[JsonPropertyName("company_id")]
	public int CompanyId { get; set; }

	[JsonPropertyName("invoice_date")]
	public string? InvoiceDate { get; set; }

	[JsonPropertyName("invoice_number")]
	public int InvoiceNumber { get; set; }

	[JsonPropertyName("user_id")]
	public int UserId { get; set; }

    /// <summary>
    /// 1- Unsettled, 2- Settled, 3- Cancelled
    /// </summary>
    [JsonPropertyName("invoice_status_id")]
	public int InvoiceStatusId { get; set; }

	[JsonPropertyName("email_notification")]
	public int EmailNotification { get; set; }

	[JsonPropertyName("settlement_date")]
	public string? SettlementDate { get; set; }

	[JsonPropertyName("cancel_date")]
	public string? CancelDate { get; set; }

	[JsonPropertyName("invoice_token")]
	public string? InvoiceToken { get; set; }

	[JsonPropertyName("invoice_payment_options_id")]
	public int? InvoicePaymentOptionsId { get; set; }

	[JsonPropertyName("reference_voucher")]
	public string? ReferenceVoucher { get; set; }

	[JsonPropertyName("late_payment_fee")]
	public int? LatePaymentFee { get; set; }

	[JsonPropertyName("email_late_payment_fee")]
	public int? EmailLatePaymentFee { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("address")]
	public string? Address { get; set; }

	[JsonPropertyName("city")]
	public string? City { get; set; }

	[JsonPropertyName("country")]
	public string? Country { get; set; }

	[JsonPropertyName("post_code")]
	public string? PostCode { get; set; }

	[JsonPropertyName("email")]
	public string? Email { get; set; }

	[JsonPropertyName("telephone")]
	public string? Telephone { get; set; }

	[JsonPropertyName("reference")]
	public string? Reference { get; set; }

	[JsonPropertyName("cancel_reason")]
	public string? CancelReason { get; set; }

	[JsonPropertyName("total_invoice")]
	public double? TotalInvoice { get; set; }

	[JsonPropertyName("total_vat")]
	public double? TotalVat { get; set; }

	[JsonPropertyName("xero_invoice_response")]
	public string? XeroInvoiceResponse { get; set; }

	[JsonPropertyName("invoice_lines")]
	public List<InvoiceLine>? InvoiceLines { get; set; } = new();

}
