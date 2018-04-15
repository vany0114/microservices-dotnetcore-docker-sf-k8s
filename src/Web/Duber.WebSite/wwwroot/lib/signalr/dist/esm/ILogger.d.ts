export declare enum LogLevel {
    Trace = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    None = 4,
}
export interface ILogger {
    log(logLevel: LogLevel, message: string): void;
}
