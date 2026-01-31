using BACApp.Core.Abstractions;
using BACApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BACApp.Core.Services;

public class MemberService : IMemberService
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public MemberService(IApiClient apiClient,
        IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<List<Member>> GetAllMembersAsync(CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var path = "/member/list/ByCompany";
        var data = await _apiClient.GetAsync<List<Member>>(path, headers, ct) ?? new List<Member>();
        return data ?? new List<Member>();
    }

    public async Task<Member> GetMemberByIdAsync(int user_id, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var path = $"/member/{user_id}";
        var data = await _apiClient.GetAsync<Member>(path, headers, ct) ?? new Member();
        return data ?? new Member();
    }
}
