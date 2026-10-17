namespace Api.Dtos.Common;

public class PagedResponse<T> where T : class
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<T> Data { get; set; } = [];
    public int TotalPages { 
        get {
            var (quotient, remainder) = Math.DivRem(TotalCount, PageSize);
            return remainder > 0 ? quotient + 1 : quotient;
        } 
    }
}
