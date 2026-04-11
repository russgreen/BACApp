using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.ViewModels;

internal partial class InvoicesPageViewModel : PageViewModel
{
    private readonly ILogger<InvoicesPageViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly IInvoiceService _invoiceService;
    private readonly IMemberService _memberService;

    private CancellationTokenSource? _invoicesCts;

    [ObservableProperty]
    private List<Member> _allMembers;

    [ObservableProperty]
    private Member _selectedMember;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private List<Invoice> _invoices;

    public InvoicesPageViewModel(ILogger<InvoicesPageViewModel> logger,
        IAuthService authService,
        IInvoiceService invoiceService,
        IMemberService memberService) : base(ApplicationPageNames.Invoices)
    {
        _logger = logger;
        _authService = authService;
        _invoiceService = invoiceService;
        _memberService = memberService;

        FromDate = DateTime.Now.AddYears(-1);
        ToDate = DateTime.Now;

        Invoices = new();

        _ = LoadInvoicesAsync(default);
    }


    [RelayCommand]
    private async Task LoadInvoicesAsync(CancellationToken ct)
    {
        if (_authService.UserCompany is null)
        {
            Invoices = new();
            return;
        }

        var from = DateOnly.FromDateTime(FromDate);
        var to = DateOnly.FromDateTime(ToDate);

        if (from > to)
        {
            (from, to) = (to, from);
        }

        try
        {
            //Invoices = (List<Invoice>)await _invoiceService.GetAllInvoicesAsync(ct);

            Invoices = (List<Invoice>)await _invoiceService.GetUnsettledInvoicesAsync(from, to, ct);
        }
        catch(Exception ex)
        {

        }
    }
}
