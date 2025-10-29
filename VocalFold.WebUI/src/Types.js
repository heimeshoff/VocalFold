import { Union, Record } from "../fable_modules/fable-library-js.4.25.0/Types.js";
import { unit_type, option_type, tuple_type, union_type, list_type, int32_type, bool_type, uint32_type, record_type, string_type } from "../fable_modules/fable-library-js.4.25.0/Reflection.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.4.25.0/Result.js";

export class KeywordReplacement extends Record {
    constructor(Keyword, Replacement) {
        super();
        this.Keyword = Keyword;
        this.Replacement = Replacement;
    }
}

export function KeywordReplacement_$reflection() {
    return record_type("Types.KeywordReplacement", [], KeywordReplacement, () => [["Keyword", string_type], ["Replacement", string_type]]);
}

export class AppSettings extends Record {
    constructor(HotkeyKey, HotkeyModifiers, IsEnabled, ModelSize, RecordingDuration, TypingSpeed, KeywordReplacements) {
        super();
        this.HotkeyKey = HotkeyKey;
        this.HotkeyModifiers = HotkeyModifiers;
        this.IsEnabled = IsEnabled;
        this.ModelSize = ModelSize;
        this.RecordingDuration = (RecordingDuration | 0);
        this.TypingSpeed = TypingSpeed;
        this.KeywordReplacements = KeywordReplacements;
    }
}

export function AppSettings_$reflection() {
    return record_type("Types.AppSettings", [], AppSettings, () => [["HotkeyKey", uint32_type], ["HotkeyModifiers", uint32_type], ["IsEnabled", bool_type], ["ModelSize", string_type], ["RecordingDuration", int32_type], ["TypingSpeed", string_type], ["KeywordReplacements", list_type(KeywordReplacement_$reflection())]]);
}

export class AppStatus extends Record {
    constructor(IsEnabled, Version, CurrentHotkey) {
        super();
        this.IsEnabled = IsEnabled;
        this.Version = Version;
        this.CurrentHotkey = CurrentHotkey;
    }
}

export function AppStatus_$reflection() {
    return record_type("Types.AppStatus", [], AppStatus, () => [["IsEnabled", bool_type], ["Version", string_type], ["CurrentHotkey", string_type]]);
}

export class Page extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Dashboard", "GeneralSettings", "KeywordSettings"];
    }
}

export function Page_$reflection() {
    return union_type("Types.Page", [], Page, () => [[], [], []]);
}

export class LoadingState$1 extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["NotStarted", "Loading", "Loaded", "Error"];
    }
}

export function LoadingState$1_$reflection(gen0) {
    return union_type("Types.LoadingState`1", [gen0], LoadingState$1, () => [[], [], [["Item", gen0]], [["Item", string_type]]]);
}

export class Toast extends Record {
    constructor(Id, Message, Type) {
        super();
        this.Id = Id;
        this.Message = Message;
        this.Type = Type;
    }
}

export function Toast_$reflection() {
    return record_type("Types.Toast", [], Toast, () => [["Id", string_type], ["Message", string_type], ["Type", ToastType_$reflection()]]);
}

export class ToastType extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Success", "Error", "Info", "Warning"];
    }
}

export function ToastType_$reflection() {
    return union_type("Types.ToastType", [], ToastType, () => [[], [], [], []]);
}

export class Model extends Record {
    constructor(CurrentPage, Settings, Status, IsRecordingHotkey, EditingKeyword, Toasts) {
        super();
        this.CurrentPage = CurrentPage;
        this.Settings = Settings;
        this.Status = Status;
        this.IsRecordingHotkey = IsRecordingHotkey;
        this.EditingKeyword = EditingKeyword;
        this.Toasts = Toasts;
    }
}

export function Model_$reflection() {
    return record_type("Types.Model", [], Model, () => [["CurrentPage", Page_$reflection()], ["Settings", LoadingState$1_$reflection(AppSettings_$reflection())], ["Status", LoadingState$1_$reflection(AppStatus_$reflection())], ["IsRecordingHotkey", bool_type], ["EditingKeyword", option_type(tuple_type(int32_type, KeywordReplacement_$reflection()))], ["Toasts", list_type(Toast_$reflection())]]);
}

export class Msg extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["NavigateToPage", "LoadSettings", "SettingsLoaded", "UpdateSettings", "SettingsSaved", "LoadStatus", "StatusLoaded", "StartRecordingHotkey", "HotkeyRecorded", "CancelRecordingHotkey", "LoadKeywords", "KeywordsLoaded", "AddKeyword", "EditKeyword", "DeleteKeyword", "SaveKeyword", "CancelEditKeyword", "AddExampleKeywords", "ExampleKeywordsAdded", "ToggleEnabled", "ShowToast", "DismissToast"];
    }
}

export function Msg_$reflection() {
    return union_type("Types.Msg", [], Msg, () => [[["Item", Page_$reflection()]], [], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [AppSettings_$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", AppSettings_$reflection()]], [["ErrorValue", string_type]]])]], [["Item", AppSettings_$reflection()]], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [unit_type, string_type], FSharpResult$2, () => [[["ResultValue", unit_type]], [["ErrorValue", string_type]]])]], [], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [AppStatus_$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", AppStatus_$reflection()]], [["ErrorValue", string_type]]])]], [], [["Item1", uint32_type], ["Item2", uint32_type]], [], [], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [list_type(KeywordReplacement_$reflection()), string_type], FSharpResult$2, () => [[["ResultValue", list_type(KeywordReplacement_$reflection())]], [["ErrorValue", string_type]]])]], [], [["Item", int32_type]], [["Item", int32_type]], [["Item", KeywordReplacement_$reflection()]], [], [], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [int32_type, string_type], FSharpResult$2, () => [[["ResultValue", int32_type]], [["ErrorValue", string_type]]])]], [], [["Item1", string_type], ["Item2", ToastType_$reflection()]], [["Item", string_type]]]);
}

