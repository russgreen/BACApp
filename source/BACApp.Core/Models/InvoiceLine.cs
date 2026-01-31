using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class InvoiceLine
{
	[JsonPropertyName("invoice_details_id")]
	public int? InvoiceDetailsId { get; set; }

	[JsonPropertyName("user_id")]
	public int? UserId { get; set; }

	[JsonPropertyName("flight_log_id")]
	public int? FlightLogId { get; set; }

	[JsonPropertyName("flight_id")]
	public int? FlightId { get; set; }

	[JsonPropertyName("flight_record")]
	public int? FlightRecord { get; set; }

	[JsonPropertyName("flight_date")]
	public string? FlightDate { get; set; }

	[JsonPropertyName("aircraft_registration")]
	public string? AircraftRegistration { get; set; }

    /// <summary>
    /// Brakes On/Off or Airborne
    /// </summary>
    [JsonPropertyName("time_type")]
	public string? TimeType { get; set; }

	[JsonPropertyName("time_from")]
	public string? TimeFrom { get; set; }

	[JsonPropertyName("time_until")]
	public string? TimeUntil { get; set; }

	[JsonPropertyName("duration")]
	public double?	 Duration { get; set; }

	[JsonPropertyName("aircraft_rate_hour")]
	public double? AircraftRateHour { get; set; }

	[JsonPropertyName("aircraft_charge_name")]
	public string? AircraftChargeName { get; set; }

	[JsonPropertyName("flight_charge")]
	public double? FlightCharge { get; set; }

	[JsonPropertyName("fuel_refund_volume")]
	public double? FuelRefundVolume { get; set; }

	[JsonPropertyName("fuel_refund_unit_price")]
	public double? FuelRefundUnitPrice { get; set; }

	[JsonPropertyName("fuel_discount")]
	public double? FuelDiscount { get; set; }

	[JsonPropertyName("vat_rate")]
	public double? VatRate { get; set; }

	[JsonPropertyName("member_discount")]
	public double? MemberDiscount { get; set; }

	[JsonPropertyName("vat_charge_rate_name")]
	public string? VatChargeRateName { get; set; }

	[JsonPropertyName("vat_charge")]
	public double? VatCharge { get; set; }

	[JsonPropertyName("total_inc_vat")]
	public double? TotalIncVat { get; set; }

	[JsonPropertyName("invoice_id")]
	public int? InvoiceId { get; set; }

	[JsonPropertyName("company_id")]
	public int? CompanyId { get; set; }

	[JsonPropertyName("flight_type")]
	public string? FlightType { get; set; }

	[JsonPropertyName("is_maintenance")]
	public int? IsMaintenance { get; set; }

	[JsonPropertyName("is_late_payment_fee")]
	public int? IsLatePaymentFee { get; set; }

	[JsonPropertyName("voucher_reference")]
	public string? VoucherReference { get; set; }

	[JsonPropertyName("landing_charge")]
	public double? LandingCharge { get; set; }

	[JsonPropertyName("tgls_charge")]
	public double? TglsCharge { get; set; }

	[JsonPropertyName("landings")]
	public int? Landings { get; set; }

	[JsonPropertyName("fuel_surcharge")]
	public double? FuelSurcharge { get; set; }

	[JsonPropertyName("tacho_start")]
	public double? TachoStart { get; set; }

	[JsonPropertyName("tacho_stop")]
	public double? TachoStop { get; set; }
}
