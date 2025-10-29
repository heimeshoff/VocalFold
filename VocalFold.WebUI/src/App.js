import { Toast, KeywordReplacement, AppSettings, ToastType, Msg, Model, LoadingState$1, Page } from "./Types.js";
import { map, filter, cons, length, item, ofArray, singleton, empty } from "../fable_modules/fable-library-js.4.25.0/List.js";
import { Cmd_OfPromise_either, Cmd_none, Cmd_batch } from "../fable_modules/Fable.Elmish.4.0.0/cmd.fs.js";
import { addExampleKeywords, deleteKeyword, addKeyword, updateKeyword, getStatus, updateSettings, getSettings } from "./Api.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.4.25.0/Result.js";
import { printf, toText } from "../fable_modules/fable-library-js.4.25.0/String.js";
import { newGuid } from "../fable_modules/fable-library-js.4.25.0/Guid.js";
import { createElement } from "react";
import React from "react";
import { createObj } from "../fable_modules/fable-library-js.4.25.0/Util.js";
import { Interop_reactApi } from "../fable_modules/Feliz.2.7.0/./Interop.fs.js";
import { singleton as singleton_1, delay, toList } from "../fable_modules/fable-library-js.4.25.0/Seq.js";
import { view as view_1 } from "./Components/GeneralSettings.js";
import { view as view_2 } from "./Components/KeywordManager.js";
import { view as view_3 } from "./Components/Dashboard.js";
import { React_useElmish_Z6C327F2E } from "../fable_modules/Feliz.UseElmish.2.4.0/./UseElmish.fs.js";
import { ProgramModule_mkProgram } from "../fable_modules/Feliz.UseElmish.2.4.0/../Fable.Elmish.4.0.0/program.fs.js";
import { createRoot } from "react-dom/client";

export function init() {
    return [new Model(new Page(0, []), new LoadingState$1(0, []), new LoadingState$1(0, []), false, undefined, empty()), Cmd_batch(ofArray([singleton((dispatch) => {
        dispatch(new Msg(1, []));
    }), singleton((dispatch_1) => {
        dispatch_1(new Msg(5, []));
    })]))];
}

export function update(msg, model) {
    let msg_1, msg_3, msg_10, msg_9, copyOfStruct;
    switch (msg.tag) {
        case 0:
            return [new Model(msg.fields[0], model.Settings, model.Status, model.IsRecordingHotkey, model.EditingKeyword, model.Toasts), Cmd_none()];
        case 1:
            return [new Model(model.CurrentPage, new LoadingState$1(1, []), model.Status, model.IsRecordingHotkey, model.EditingKeyword, model.Toasts), Cmd_OfPromise_either(getSettings, undefined, (Item) => (new Msg(2, [Item])), (ex) => (new Msg(2, [new FSharpResult$2(1, [ex.message])])))];
        case 2:
            if (msg.fields[0].tag === 1) {
                return [new Model(model.CurrentPage, new LoadingState$1(3, [msg.fields[0].fields[0]]), model.Status, model.IsRecordingHotkey, model.EditingKeyword, model.Toasts), (msg_1 = (new Msg(20, [toText(printf("Failed to load settings: %s"))(msg.fields[0].fields[0]), new ToastType(1, [])])), singleton((dispatch) => {
                    dispatch(msg_1);
                }))];
            }
            else {
                return [new Model(model.CurrentPage, new LoadingState$1(2, [msg.fields[0].fields[0]]), model.Status, model.IsRecordingHotkey, model.EditingKeyword, model.Toasts), Cmd_none()];
            }
        case 3:
            return [model, Cmd_OfPromise_either(updateSettings, msg.fields[0], (Item_1) => (new Msg(4, [Item_1])), (ex_1) => (new Msg(4, [new FSharpResult$2(1, [ex_1.message])])))];
        case 4:
            if (msg.fields[0].tag === 1) {
                return [model, (msg_3 = (new Msg(20, [toText(printf("Failed to save settings: %s"))(msg.fields[0].fields[0]), new ToastType(1, [])])), singleton((dispatch_2) => {
                    dispatch_2(msg_3);
                }))];
            }
            else {
                return [model, singleton((dispatch_1) => {
                    dispatch_1(new Msg(1, []));
                })];
            }
        case 5:
            return [new Model(model.CurrentPage, model.Settings, new LoadingState$1(1, []), model.IsRecordingHotkey, model.EditingKeyword, model.Toasts), Cmd_OfPromise_either(getStatus, undefined, (Item_2) => (new Msg(6, [Item_2])), (ex_2) => (new Msg(6, [new FSharpResult$2(1, [ex_2.message])])))];
        case 6:
            if (msg.fields[0].tag === 1) {
                return [new Model(model.CurrentPage, model.Settings, new LoadingState$1(3, [msg.fields[0].fields[0]]), model.IsRecordingHotkey, model.EditingKeyword, model.Toasts), Cmd_none()];
            }
            else {
                return [new Model(model.CurrentPage, model.Settings, new LoadingState$1(2, [msg.fields[0].fields[0]]), model.IsRecordingHotkey, model.EditingKeyword, model.Toasts), Cmd_none()];
            }
        case 19: {
            const matchValue = model.Settings;
            if (matchValue.tag === 2) {
                const settings_3 = matchValue.fields[0];
                const updatedSettings = new AppSettings(settings_3.HotkeyKey, settings_3.HotkeyModifiers, !settings_3.IsEnabled, settings_3.ModelSize, settings_3.RecordingDuration, settings_3.TypingSpeed, settings_3.KeywordReplacements);
                return [model, singleton((dispatch_3) => {
                    dispatch_3(new Msg(3, [updatedSettings]));
                })];
            }
            else {
                return [model, Cmd_none()];
            }
        }
        case 7:
            return [new Model(model.CurrentPage, model.Settings, model.Status, true, model.EditingKeyword, model.Toasts), Cmd_none()];
        case 8: {
            const matchValue_1 = model.Settings;
            if (matchValue_1.tag === 2) {
                const settings_4 = matchValue_1.fields[0];
                if (msg.fields[0] === 0) {
                    return [new Model(model.CurrentPage, model.Settings, model.Status, false, model.EditingKeyword, model.Toasts), singleton((dispatch_4) => {
                        dispatch_4(new Msg(20, ["Hotkey must include at least one modifier key (Ctrl, Shift, Alt, or Win)", new ToastType(3, [])]));
                    })];
                }
                else {
                    const updatedSettings_1 = new AppSettings(msg.fields[1], msg.fields[0], settings_4.IsEnabled, settings_4.ModelSize, settings_4.RecordingDuration, settings_4.TypingSpeed, settings_4.KeywordReplacements);
                    return [new Model(model.CurrentPage, model.Settings, model.Status, false, model.EditingKeyword, model.Toasts), Cmd_batch(ofArray([singleton((dispatch_5) => {
                        dispatch_5(new Msg(3, [updatedSettings_1]));
                    }), singleton((dispatch_6) => {
                        dispatch_6(new Msg(20, ["Hotkey updated successfully", new ToastType(0, [])]));
                    })]))];
                }
            }
            else {
                return [new Model(model.CurrentPage, model.Settings, model.Status, false, model.EditingKeyword, model.Toasts), Cmd_none()];
            }
        }
        case 9:
            return [new Model(model.CurrentPage, model.Settings, model.Status, false, model.EditingKeyword, model.Toasts), Cmd_none()];
        case 12:
            return [new Model(model.CurrentPage, model.Settings, model.Status, model.IsRecordingHotkey, [-1, new KeywordReplacement("", "")], model.Toasts), Cmd_none()];
        case 13: {
            const matchValue_2 = model.Settings;
            let matchResult, settings_6;
            if (matchValue_2.tag === 2) {
                if ((msg.fields[0] >= 0) && (msg.fields[0] < length(matchValue_2.fields[0].KeywordReplacements))) {
                    matchResult = 0;
                    settings_6 = matchValue_2.fields[0];
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0:
                    return [new Model(model.CurrentPage, model.Settings, model.Status, model.IsRecordingHotkey, [msg.fields[0], item(msg.fields[0], settings_6.KeywordReplacements)], model.Toasts), Cmd_none()];
                default:
                    return [model, Cmd_none()];
            }
        }
        case 15: {
            const matchValue_3 = model.Settings;
            const matchValue_4 = model.EditingKeyword;
            let matchResult_1, index_2, settings_8, settings_9;
            if (matchValue_3.tag === 2) {
                if (matchValue_4 != null) {
                    if (matchValue_4[0] >= 0) {
                        matchResult_1 = 0;
                        index_2 = matchValue_4[0];
                        settings_8 = matchValue_3.fields[0];
                    }
                    else if (matchValue_4[0] === -1) {
                        matchResult_1 = 1;
                        settings_9 = matchValue_3.fields[0];
                    }
                    else {
                        matchResult_1 = 2;
                    }
                }
                else {
                    matchResult_1 = 2;
                }
            }
            else {
                matchResult_1 = 2;
            }
            switch (matchResult_1) {
                case 0:
                    return [model, Cmd_OfPromise_either((keyword_1) => updateKeyword(index_2, keyword_1), msg.fields[0], (_arg) => (new Msg(4, [new FSharpResult$2(0, [undefined])])), (ex_3) => (new Msg(4, [new FSharpResult$2(1, [ex_3.message])])))];
                case 1:
                    return [model, Cmd_OfPromise_either(addKeyword, msg.fields[0], (_arg_1) => (new Msg(4, [new FSharpResult$2(0, [undefined])])), (ex_4) => (new Msg(4, [new FSharpResult$2(1, [ex_4.message])])))];
                default:
                    return [model, Cmd_none()];
            }
        }
        case 14:
            return [model, Cmd_OfPromise_either(deleteKeyword, msg.fields[0], (_arg_2) => (new Msg(4, [new FSharpResult$2(0, [undefined])])), (ex_5) => (new Msg(4, [new FSharpResult$2(1, [ex_5.message])])))];
        case 16:
            return [new Model(model.CurrentPage, model.Settings, model.Status, model.IsRecordingHotkey, undefined, model.Toasts), Cmd_none()];
        case 17:
            return [model, Cmd_OfPromise_either(addExampleKeywords, undefined, (Item_3) => (new Msg(18, [Item_3])), (ex_6) => (new Msg(18, [new FSharpResult$2(1, [ex_6.message])])))];
        case 18:
            if (msg.fields[0].tag === 1) {
                return [model, (msg_10 = (new Msg(20, [toText(printf("Failed to add examples: %s"))(msg.fields[0].fields[0]), new ToastType(1, [])])), singleton((dispatch_9) => {
                    dispatch_9(msg_10);
                }))];
            }
            else {
                return [model, Cmd_batch(ofArray([singleton((dispatch_7) => {
                    dispatch_7(new Msg(1, []));
                }), (msg_9 = (new Msg(20, [toText(printf("Added %d example keywords"))(msg.fields[0].fields[0]), new ToastType(0, [])])), singleton((dispatch_8) => {
                    dispatch_8(msg_9);
                }))]))];
            }
        case 20:
            return [new Model(model.CurrentPage, model.Settings, model.Status, model.IsRecordingHotkey, model.EditingKeyword, cons(new Toast((copyOfStruct = newGuid(), copyOfStruct), msg.fields[0], msg.fields[1]), model.Toasts)), Cmd_none()];
        case 21:
            return [new Model(model.CurrentPage, model.Settings, model.Status, model.IsRecordingHotkey, model.EditingKeyword, filter((t) => (t.Id !== msg.fields[0]), model.Toasts)), Cmd_none()];
        default:
            return [model, Cmd_none()];
    }
}

function view(model, dispatch) {
    let elems_6, elems_3, elems_1, elems, elems_2, elems_5;
    return createElement("div", createObj(ofArray([["className", "min-h-screen bg-background-dark text-text-primary"], (elems_6 = [createElement("div", createObj(ofArray([["className", "flex"], (elems_3 = [createElement("aside", createObj(ofArray([["className", "w-64 h-screen bg-background-sidebar p-6"], (elems_1 = [createElement("h1", {
        className: "text-2xl font-bold text-primary mb-8",
        children: "VocalFold",
    }), createElement("nav", createObj(singleton((elems = [createElement("button", {
        className: "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth",
        children: "Dashboard",
        onClick: (_arg) => {
            dispatch(new Msg(0, [new Page(0, [])]));
        },
    }), createElement("button", {
        className: "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth",
        children: "General Settings",
        onClick: (_arg_1) => {
            dispatch(new Msg(0, [new Page(1, [])]));
        },
    }), createElement("button", {
        className: "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth",
        children: "Keywords",
        onClick: (_arg_2) => {
            dispatch(new Msg(0, [new Page(2, [])]));
        },
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))]))))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])]))), createElement("main", createObj(ofArray([["className", "flex-1 p-8"], (elems_2 = toList(delay(() => {
        const matchValue = model.CurrentPage;
        return (matchValue.tag === 1) ? singleton_1(view_1(model.Settings, model.IsRecordingHotkey, dispatch)) : ((matchValue.tag === 2) ? singleton_1(view_2(model.Settings, model.EditingKeyword, dispatch)) : singleton_1(view_3(model.Status, model.Settings, dispatch)));
    })), ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])]))), createElement("div", createObj(ofArray([["className", "fixed top-4 right-4 space-y-2"], (elems_5 = map((toast) => {
        let elems_4;
        let toastBorderColor;
        const matchValue_1 = toast.Type;
        toastBorderColor = ((matchValue_1.tag === 1) ? "border-red-500" : ((matchValue_1.tag === 2) ? "border-blue-500" : ((matchValue_1.tag === 3) ? "border-yellow-500" : "border-green-500")));
        return createElement("div", createObj(ofArray([["key", toast.Id], ["className", toText(printf("bg-background-card p-4 rounded shadow-lg border-l-4 %s"))(toastBorderColor)], (elems_4 = [createElement("p", {
            children: [toast.Message],
        }), createElement("button", {
            className: "text-sm text-text-secondary hover:text-text-primary",
            children: "Dismiss",
            onClick: (_arg_3) => {
                dispatch(new Msg(21, [toast.Id]));
            },
        })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_4))])])));
    }, model.Toasts), ["children", Interop_reactApi.Children.toArray(Array.from(elems_5))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_6))])])));
}

export function App() {
    const patternInput = React_useElmish_Z6C327F2E(() => ProgramModule_mkProgram(init, update, (_arg, _arg_1) => {
    }), undefined, []);
    return view(patternInput[0], patternInput[1]);
}

export const root = document.getElementById("root");

export const app = createRoot(root);

app.render(createElement(App, null));

