export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
    statusCode: number;
}
export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}
