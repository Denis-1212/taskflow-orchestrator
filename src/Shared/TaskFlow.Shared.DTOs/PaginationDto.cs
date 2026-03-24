namespace TaskFlow.Shared.DTOs;

public record PaginatedRequestDto(int Page = 1, int PageSize = 20);
public record PaginatedResponseDto<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
