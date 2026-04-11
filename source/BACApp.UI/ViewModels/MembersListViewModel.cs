using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.ViewModels;

internal partial class MembersListViewModel : PageViewModel
{
    private readonly ILogger<MembersListViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly IInvoiceService _invoiceService;
    private readonly IMemberService _memberService;
    private readonly IFlightLogsService _flightLogsService;
    private readonly IAircraftService _aircraftService;
    private readonly ITechlogService _techlogService;

    private CancellationTokenSource? _invoicesCts;

    [ObservableProperty]
    private ObservableCollection<MemberEx> _allMembers = new();

    [ObservableProperty]
    private MemberEx _selectedMember;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private List<Invoice> _allInvoices = new();

    [ObservableProperty]
    private List<FlightLog> _allFlightLogs = new();

    [ObservableProperty]
    private List<TechLog> _allTechLogs = new();

    public MembersListViewModel(ILogger<MembersListViewModel> logger,
        IAuthService authService,
        IInvoiceService invoiceService,
        IMemberService memberService,
        IFlightLogsService flightLogsService,
        IAircraftService aircraftService,
        ITechlogService techlogService) : base(ApplicationPageNames.Members)
    {
        _logger = logger;
        _authService = authService;
        _invoiceService = invoiceService;
        _memberService = memberService;
        _aircraftService = aircraftService;
        _flightLogsService = flightLogsService;
        _techlogService = techlogService;

        FromDate = DateTime.Now.AddYears(-1);
        ToDate = DateTime.Now;

        LoadAsync().ConfigureAwait(false);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        if (_authService.UserCompany is null)
        {
            return;
        }

        var from = DateOnly.FromDateTime(FromDate);
        var to = DateOnly.FromDateTime(ToDate);

        if (from > to)
        {
            (from, to) = (to, from);
        }

        var invoicesTask = _invoiceService.GetUnsettledInvoicesAsync(from, to, ct);
        //var flightLogsTask = _flightLogsService.GetFlightLogsUnbilledAsync(ct);
        //var techLogsTask = _techlogService.GetAllTechLogsAsync(ct);
        var membersTask = _memberService.GetAllMembersAsync(ct);

        //await Task.WhenAll(invoicesTask, flightLogsTask, techLogsTask, membersTask);
        await Task.WhenAll(invoicesTask, membersTask);

        var invoices = invoicesTask.Result.ToList();
        //var flightLogs = flightLogsTask.Result.ToList();
        //var techLogs = techLogsTask.Result.ToList();
        var members = membersTask.Result;

        AllInvoices = invoices;
        //AllFlightLogs = flightLogs;
        //AllTechLogs = techLogs;

        var invoicesByUserId = invoices
            .Where(i => i.UserId.HasValue)
            .GroupBy(i => i.UserId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        //var flightLogsById = flightLogs
        //    .GroupBy(f => f.FlightLogId)
        //    .ToDictionary(g => g.Key, g => g.First());

        //var flightLogIdsByMemberId = new Dictionary<int, HashSet<int>>();

        //foreach (var techLog in techLogs)
        //{
        //    var memberId = techLog.StudentId ?? techLog.PicId;
        //    if (!memberId.HasValue || !techLog.FlightLogId.HasValue)
        //    {
        //        continue;
        //    }

        //    if (!flightLogIdsByMemberId.TryGetValue(memberId.Value, out var logIds))
        //    {
        //        logIds = new HashSet<int>();
        //        flightLogIdsByMemberId[memberId.Value] = logIds;
        //    }

        //    logIds.Add(techLog.FlightLogId.Value);
        //}

        var loadedMembers = new List<MemberEx>(members.Count);

        foreach (var member in members)
        {
            var memberEx = new MemberEx(member);

            if (invoicesByUserId.TryGetValue(member.UserId, out var memberInvoices))
            {
                memberEx.UnsettledInvoices = memberInvoices;
                memberEx.UnsettledInvoicesAmount = (decimal)memberInvoices.Sum(i => i.TotalInvoice ?? 0);
            }
            else
            {
                memberEx.UnsettledInvoices = new List<Invoice>();
                memberEx.UnsettledInvoicesAmount = 0;
            }

            //if (flightLogIdsByMemberId.TryGetValue(member.UserId, out var memberFlightLogIds))
            //{
            //    var memberFlightLogs = memberFlightLogIds
            //        .Where(flightLogsById.ContainsKey)
            //        .Select(flightLogId => flightLogsById[flightLogId])
            //        .ToList();

            //    memberEx.UnpaidFlightLogs = memberFlightLogs;
            //    memberEx.UnpaidFlightHours = memberFlightLogs.Sum(f => f.ChargeTimeDecimal);
            //}
            //else
            //{
            //    memberEx.UnpaidFlightLogs = new List<FlightLog>();
            //    memberEx.UnpaidFlightHours = 0;
            //}

            loadedMembers.Add(memberEx);
        }

        AllMembers = new ObservableCollection<MemberEx>(loadedMembers);
    }


}
