export interface ErrorResponse {
    success: boolean;
    message: string;
    error: string;
    traceId: string;
    time: Date;
}
