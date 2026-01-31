using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface IMemberService
{
    /// <summary>
    /// Get all members in the company
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<Member>> GetAllMembersAsync(CancellationToken ct = default);

    /// <summary>
    /// Get member by user id
    /// </summary>
    /// <param name="user_id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Member> GetMemberByIdAsync(int user_id, CancellationToken ct = default);
}
