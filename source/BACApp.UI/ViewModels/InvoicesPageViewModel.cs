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

        FromDate = DateTime.Now.AddMonths(-1);
        ToDate = DateTime.Now;

        Invoices = new();

        _ = LoadAsync();
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        if (_authService.UserCompany is null)
        {
            return;
        }

        var members = await _memberService.GetAllMembersAsync(ct);

        AllMembers = members
            .OrderBy(a => a.Name)
            .ToList();

        if (AllMembers != null && AllMembers.Count > 0)
        {
            SelectedMember = AllMembers.First();
            await LoadInvoicesAsync(ct);
        }
    }

    [RelayCommand]
    private async Task LoadInvoicesAsync(CancellationToken ct)
    {
        if (_authService.UserCompany is null || SelectedMember is null)
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
            Invoices = (List<Invoice>)await _invoiceService.GetInvoicesAsync(SelectedMember.UserId, from, to, ct);
        }
        catch(Exception ex)
        {

        }
    }
}
