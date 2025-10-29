import { fromString, list, int, bool, uint32, string, object } from "../fable_modules/Thoth.Json.10.4.1/Decode.fs.js";
import { AppStatus, AppSettings, KeywordReplacement } from "./Types.fs.js";
import { toString, list as list_1, object as object_1 } from "../fable_modules/Thoth.Json.10.4.1/Encode.fs.js";
import { uncurry2 } from "../fable_modules/fable-library-js.4.25.0/Util.js";
import { map } from "../fable_modules/fable-library-js.4.25.0/List.js";
import { startAsPromise } from "../fable_modules/fable-library-js.4.25.0/Async.js";
import { singleton } from "../fable_modules/fable-library-js.4.25.0/AsyncBuilder.js";
import { Headers_contentType, Http_header, Http_content, Http_request, Http_method, Http_send } from "../fable_modules/Fable.SimpleHttp.3.6.0/Http.fs.js";
import { BodyContent, HttpMethod } from "../fable_modules/Fable.SimpleHttp.3.6.0/Types.fs.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.4.25.0/Result.js";
import { printf, toText } from "../fable_modules/fable-library-js.4.25.0/String.js";

export const keywordReplacementDecoder = (path_2) => ((v) => object((get$) => {
    let objectArg, objectArg_1;
    return new KeywordReplacement((objectArg = get$.Required, objectArg.Field("keyword", string)), (objectArg_1 = get$.Required, objectArg_1.Field("replacement", string)));
}, path_2, v));

export function keywordReplacementEncoder(kr) {
    return object_1([["keyword", kr.Keyword], ["replacement", kr.Replacement]]);
}

export const appSettingsDecoder = (path_4) => ((v) => object((get$) => {
    let objectArg, objectArg_1, objectArg_2, objectArg_3, objectArg_4, objectArg_5, objectArg_6;
    return new AppSettings((objectArg = get$.Required, objectArg.Field("hotkeyKey", uncurry2(uint32))), (objectArg_1 = get$.Required, objectArg_1.Field("hotkeyModifiers", uncurry2(uint32))), (objectArg_2 = get$.Required, objectArg_2.Field("isEnabled", bool)), (objectArg_3 = get$.Required, objectArg_3.Field("modelSize", string)), (objectArg_4 = get$.Required, objectArg_4.Field("recordingDuration", uncurry2(int))), (objectArg_5 = get$.Required, objectArg_5.Field("typingSpeedStr", string)), (objectArg_6 = get$.Required, objectArg_6.Field("keywordReplacements", (path_3, value_3) => list(uncurry2(keywordReplacementDecoder), path_3, value_3))));
}, path_4, v));

export function appSettingsEncoder(settings) {
    return object_1([["hotkeyKey", settings.HotkeyKey], ["hotkeyModifiers", settings.HotkeyModifiers], ["isEnabled", settings.IsEnabled], ["modelSize", settings.ModelSize], ["recordingDuration", settings.RecordingDuration], ["typingSpeedStr", settings.TypingSpeed], ["keywordReplacements", list_1(map(keywordReplacementEncoder, settings.KeywordReplacements))]]);
}

export const appStatusDecoder = (path_3) => ((v) => object((get$) => {
    let objectArg, objectArg_1, objectArg_2;
    return new AppStatus((objectArg = get$.Required, objectArg.Field("isEnabled", bool)), (objectArg_1 = get$.Required, objectArg_1.Field("version", string)), (objectArg_2 = get$.Required, objectArg_2.Field("currentHotkey", string)));
}, path_3, v));

const baseUrl = "";

/**
 * Get current application status
 */
export function getStatus() {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => singleton.Bind(Http_send(Http_method(new HttpMethod(0, []), Http_request(baseUrl + "/api/status"))), (_arg) => {
        const response = _arg;
        const matchValue = response.statusCode | 0;
        if (matchValue === 200) {
            const matchValue_1 = fromString(uncurry2(appStatusDecoder), response.responseText);
            return (matchValue_1.tag === 1) ? singleton.Return(new FSharpResult$2(1, [matchValue_1.fields[0]])) : singleton.Return(new FSharpResult$2(0, [matchValue_1.fields[0]]));
        }
        else {
            return singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
        }
    })), (_arg_1) => {
        let arg_2;
        return singleton.Return(new FSharpResult$2(1, [(arg_2 = _arg_1.message, toText(printf("Failed to get status: %s"))(arg_2))]));
    })));
}

/**
 * Get current settings
 */
export function getSettings() {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => singleton.Bind(Http_send(Http_method(new HttpMethod(0, []), Http_request(baseUrl + "/api/settings"))), (_arg) => {
        const response = _arg;
        const matchValue = response.statusCode | 0;
        if (matchValue === 200) {
            const matchValue_1 = fromString(uncurry2(appSettingsDecoder), response.responseText);
            return (matchValue_1.tag === 1) ? singleton.Return(new FSharpResult$2(1, [matchValue_1.fields[0]])) : singleton.Return(new FSharpResult$2(0, [matchValue_1.fields[0]]));
        }
        else {
            return singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
        }
    })), (_arg_1) => {
        let arg_2;
        return singleton.Return(new FSharpResult$2(1, [(arg_2 = _arg_1.message, toText(printf("Failed to get settings: %s"))(arg_2))]));
    })));
}

/**
 * Update settings
 */
export function updateSettings(settings) {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => {
        let req_2;
        const json = toString(0, appSettingsEncoder(settings));
        return singleton.Bind(Http_send((req_2 = Http_content(new BodyContent(1, [json]), Http_method(new HttpMethod(2, []), Http_request(baseUrl + "/api/settings"))), Http_header(Headers_contentType("application/json"), req_2))), (_arg) => {
            const response = _arg;
            const matchValue = response.statusCode | 0;
            return (matchValue === 200) ? singleton.Return(new FSharpResult$2(0, [undefined])) : singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
        });
    }), (_arg_1) => {
        let arg_2;
        return singleton.Return(new FSharpResult$2(1, [(arg_2 = _arg_1.message, toText(printf("Failed to update settings: %s"))(arg_2))]));
    })));
}

/**
 * Get all keywords
 */
export function getKeywords() {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => singleton.Bind(Http_send(Http_method(new HttpMethod(0, []), Http_request(baseUrl + "/api/keywords"))), (_arg) => {
        const response = _arg;
        const matchValue = response.statusCode | 0;
        if (matchValue === 200) {
            const matchValue_1 = fromString((path, value) => list(uncurry2(keywordReplacementDecoder), path, value), response.responseText);
            return (matchValue_1.tag === 1) ? singleton.Return(new FSharpResult$2(1, [matchValue_1.fields[0]])) : singleton.Return(new FSharpResult$2(0, [matchValue_1.fields[0]]));
        }
        else {
            return singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
        }
    })), (_arg_1) => {
        let arg_2;
        return singleton.Return(new FSharpResult$2(1, [(arg_2 = _arg_1.message, toText(printf("Failed to get keywords: %s"))(arg_2))]));
    })));
}

/**
 * Add a new keyword
 */
export function addKeyword(keyword) {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => {
        let req_2;
        const json = toString(0, keywordReplacementEncoder(keyword));
        return singleton.Bind(Http_send((req_2 = Http_content(new BodyContent(1, [json]), Http_method(new HttpMethod(1, []), Http_request(baseUrl + "/api/keywords"))), Http_header(Headers_contentType("application/json"), req_2))), (_arg) => {
            const response = _arg;
            const matchValue = response.statusCode | 0;
            return (matchValue === 200) ? singleton.Return(new FSharpResult$2(0, [undefined])) : singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
        });
    }), (_arg_1) => {
        let arg_2;
        return singleton.Return(new FSharpResult$2(1, [(arg_2 = _arg_1.message, toText(printf("Failed to add keyword: %s"))(arg_2))]));
    })));
}

/**
 * Update a keyword at the given index
 */
export function updateKeyword(index, keyword) {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => {
        let req_2;
        const json = toString(0, keywordReplacementEncoder(keyword));
        return singleton.Bind(Http_send((req_2 = Http_content(new BodyContent(1, [json]), Http_method(new HttpMethod(2, []), Http_request(baseUrl + toText(printf("/api/keywords/%d"))(index)))), Http_header(Headers_contentType("application/json"), req_2))), (_arg) => {
            const response = _arg;
            const matchValue = response.statusCode | 0;
            return (matchValue === 200) ? singleton.Return(new FSharpResult$2(0, [undefined])) : singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
        });
    }), (_arg_1) => {
        let arg_3;
        return singleton.Return(new FSharpResult$2(1, [(arg_3 = _arg_1.message, toText(printf("Failed to update keyword: %s"))(arg_3))]));
    })));
}

/**
 * Delete a keyword at the given index
 */
export function deleteKeyword(index) {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => singleton.Bind(Http_send(Http_method(new HttpMethod(4, []), Http_request(baseUrl + toText(printf("/api/keywords/%d"))(index)))), (_arg) => {
        const response = _arg;
        const matchValue = response.statusCode | 0;
        return (matchValue === 200) ? singleton.Return(new FSharpResult$2(0, [undefined])) : singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
    })), (_arg_1) => {
        let arg_3;
        return singleton.Return(new FSharpResult$2(1, [(arg_3 = _arg_1.message, toText(printf("Failed to delete keyword: %s"))(arg_3))]));
    })));
}

/**
 * Add example keywords
 */
export function addExampleKeywords() {
    return startAsPromise(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => singleton.Bind(Http_send(Http_method(new HttpMethod(1, []), Http_request(baseUrl + "/api/keywords/examples"))), (_arg) => {
        const response = _arg;
        const matchValue = response.statusCode | 0;
        if (matchValue === 200) {
            const matchValue_1 = fromString((path, v) => object((get$) => {
                const objectArg = get$.Required;
                return objectArg.Field("added", uncurry2(int)) | 0;
            }, path, v), response.responseText);
            return (matchValue_1.tag === 1) ? singleton.Return(new FSharpResult$2(1, [matchValue_1.fields[0]])) : singleton.Return(new FSharpResult$2(0, [matchValue_1.fields[0]]));
        }
        else {
            return singleton.Return(new FSharpResult$2(1, [toText(printf("HTTP %d: %s"))(matchValue)(response.responseText)]));
        }
    })), (_arg_1) => {
        let arg_4;
        return singleton.Return(new FSharpResult$2(1, [(arg_4 = _arg_1.message, toText(printf("Failed to add example keywords: %s"))(arg_4))]));
    })));
}

