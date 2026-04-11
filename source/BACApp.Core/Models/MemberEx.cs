using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public partial class MemberEx : ObservableObject
{
    [ObservableProperty]
    private Member _member;

    [ObservableProperty]
    private List<Invoice>? _unsettledInvoices = new();

    [ObservableProperty]
    private List<FlightLog>? _unpaidFlightLogs = new();

    [ObservableProperty]
    private double _unpaidFlightHours;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAmountOwed))]
    private decimal _unpaidFlightsAmount;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAmountOwed))]
    private decimal _unsettledInvoicesAmount;

    public decimal TotalAmountOwed => UnpaidFlightsAmount + UnsettledInvoicesAmount;

    public MemberEx(Member member)
    {
        _member = member;
    }
}
