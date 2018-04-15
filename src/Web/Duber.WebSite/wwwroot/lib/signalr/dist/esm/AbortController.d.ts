export declare class AbortController implements AbortSignal {
    private isAborted;
    onabort: () => void;
    abort(): void;
    readonly signal: AbortSignal;
    readonly aborted: boolean;
}
export interface AbortSignal {
    aborted: boolean;
    onabort: () => void;
}
