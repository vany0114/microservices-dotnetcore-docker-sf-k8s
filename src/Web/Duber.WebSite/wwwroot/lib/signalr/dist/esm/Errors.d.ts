export declare class HttpError extends Error {
    private __proto__;
    statusCode: number;
    constructor(errorMessage: string, statusCode: number);
}
export declare class TimeoutError extends Error {
    private __proto__;
    constructor(errorMessage?: string);
}
