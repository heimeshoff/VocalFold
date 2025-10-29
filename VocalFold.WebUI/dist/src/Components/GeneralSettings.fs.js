import { isNullOrEmpty, join, printf, toText } from "../../fable_modules/fable-library-js.4.25.0/String.js";
import { createElement } from "react";
import React from "react";
import { React_createDisposable_3A5B6456, useReact_useEffect_7331F961 } from "../../fable_modules/Feliz.2.7.0/React.fs.js";
import { AppSettings, Msg } from "../Types.fs.js";
import { CardProps, card } from "./Card.fs.js";
import { createObj } from "../../fable_modules/fable-library-js.4.25.0/Util.js";
import { singleton, append, delay, toList } from "../../fable_modules/fable-library-js.4.25.0/Seq.js";
import { Interop_reactApi } from "../../fable_modules/Feliz.2.7.0/./Interop.fs.js";
import { map, singleton as singleton_1, ofArray } from "../../fable_modules/fable-library-js.4.25.0/List.js";

export function getKeyDisplayName(keyCode) {
    let k, k_1;
    switch (keyCode) {
        case 8:
            return "Backspace";
        case 9:
            return "Tab";
        case 13:
            return "Enter";
        case 27:
            return "Escape";
        case 32:
            return "Space";
        case 112:
            return "F1";
        case 113:
            return "F2";
        case 114:
            return "F3";
        case 115:
            return "F4";
        case 116:
            return "F5";
        case 117:
            return "F6";
        case 118:
            return "F7";
        case 119:
            return "F8";
        case 120:
            return "F9";
        case 121:
            return "F10";
        case 122:
            return "F11";
        case 123:
            return "F12";
        default:
            if ((k = keyCode, (k >= 65) && (k <= 90))) {
                return String.fromCharCode(keyCode);
            }
            else if ((k_1 = keyCode, (k_1 >= 48) && (k_1 <= 57))) {
                return String.fromCharCode(keyCode);
            }
            else {
                return toText(printf("Key%d"))(keyCode);
            }
    }
}

export function getModifiersDisplay(modifiers) {
    const parts = [];
    if (((modifiers & 1) >>> 0) !== 0) {
        void (parts.push("Alt"));
    }
    if (((modifiers & 2) >>> 0) !== 0) {
        void (parts.push("Ctrl"));
    }
    if (((modifiers & 4) >>> 0) !== 0) {
        void (parts.push("Shift"));
    }
    if (((modifiers & 8) >>> 0) !== 0) {
        void (parts.push("Win"));
    }
    return join("+", parts);
}

export function formatHotkey(modifiers, key) {
    const modDisplay = getModifiersDisplay(modifiers);
    const keyDisplay = getKeyDisplayName(key);
    if (isNullOrEmpty(modDisplay)) {
        return keyDisplay;
    }
    else {
        return toText(printf("%s+%s"))(modDisplay)(keyDisplay);
    }
}

function hotkeyRecorder(hotkeyRecorderInputProps) {
    let elems_4;
    const dispatch = hotkeyRecorderInputProps.dispatch;
    const currentModifiers = hotkeyRecorderInputProps.currentModifiers;
    const currentKey = hotkeyRecorderInputProps.currentKey;
    const isRecording = hotkeyRecorderInputProps.isRecording;
    useReact_useEffect_7331F961(() => {
        if (isRecording) {
            const handler = (e_1) => {
                const e = e_1;
                if (isRecording) {
                    e.preventDefault();
                    e.stopPropagation();
                    const modifiers = ((((((e.altKey ? 1 : 0) | (e.ctrlKey ? 2 : 0)) >>> 0) | (e.shiftKey ? 4 : 0)) >>> 0) | (e.metaKey ? 8 : 0)) >>> 0;
                    const keyCode = e.keyCode >>> 0;
                    if ((((keyCode !== 16) && (keyCode !== 17)) && (keyCode !== 18)) && (keyCode !== 91)) {
                        dispatch(new Msg(8, [modifiers, keyCode]));
                    }
                }
            };
            window.addEventListener("keydown", handler);
            return React_createDisposable_3A5B6456(() => {
                window.removeEventListener("keydown", handler);
            });
        }
        else {
            return React_createDisposable_3A5B6456(() => {
            });
        }
    }, [isRecording]);
    return card(new CardProps("Global Hotkey", singleton_1(createElement("div", createObj(ofArray([["className", "space-y-4"], (elems_4 = toList(delay(() => {
        let elems;
        return append(singleton(createElement("div", createObj(ofArray([["className", "flex items-center justify-between"], (elems = toList(delay(() => {
            let children;
            return append(singleton((children = toList(delay(() => append(singleton(createElement("p", {
                className: "text-text-secondary text-sm mb-1",
                children: "Current Hotkey",
            })), delay(() => {
                let arg;
                return singleton(createElement("p", {
                    className: (arg = (isRecording ? "text-amber-500 animate-pulse" : "text-primary"), toText(printf("text-2xl font-mono font-bold %s"))(arg)),
                    children: formatHotkey(currentModifiers, currentKey),
                }));
            })))), createElement("div", {
                children: Interop_reactApi.Children.toArray(Array.from(children)),
            }))), delay(() => {
                let arg_1;
                return singleton(createElement("button", {
                    className: (arg_1 = (isRecording ? "bg-red-500 hover:bg-red-600 text-white animate-pulse" : "bg-primary hover:bg-primary-600 text-white"), toText(printf("px-6 py-2 rounded-lg font-medium transition-all duration-200 %s"))(arg_1)),
                    children: isRecording ? "Cancel" : "Record New Hotkey",
                    onClick: (_arg) => {
                        if (isRecording) {
                            dispatch(new Msg(9, []));
                        }
                        else {
                            dispatch(new Msg(7, []));
                        }
                    },
                }));
            }));
        })), ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))), delay(() => {
            let elems_2, elems_1, elems_3, value_37;
            return isRecording ? singleton(createElement("div", createObj(ofArray([["className", "bg-amber-500/10 border border-amber-500/30 rounded p-4"], (elems_2 = [createElement("p", createObj(ofArray([["className", "text-amber-500 font-medium flex items-center"], (elems_1 = [createElement("span", {
                className: "text-xl mr-2",
                children: "🔴",
            }), createElement("span", {
                children: ["Press any key combination..."],
            })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])]))), createElement("p", {
                className: "text-text-secondary text-sm mt-2",
                children: "Tip: Must include at least one modifier key (Ctrl, Shift, Alt, or Win)",
            })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])])))) : singleton(createElement("div", createObj(ofArray([["className", "bg-primary/10 border border-primary/30 rounded p-4"], (elems_3 = [createElement("p", createObj(ofArray([["className", "text-text-secondary text-sm"], (value_37 = "Click \'Record New Hotkey\' and press your desired key combination. The hotkey will be saved automatically.", ["children", value_37])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])]))));
        }));
    })), ["children", Interop_reactApi.Children.toArray(Array.from(elems_4))])])))), undefined));
}

function modelSelector(currentModel, dispatch, settings) {
    let elems_3, value_2, elems, elems_2, elems_1;
    return card(new CardProps("Whisper Model", singleton_1(createElement("div", createObj(ofArray([["className", "space-y-4"], (elems_3 = [createElement("select", createObj(ofArray([(value_2 = "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors", ["className", value_2]), ["value", currentModel], ["onChange", (ev) => {
        dispatch(new Msg(3, [new AppSettings(settings.HotkeyKey, settings.HotkeyModifiers, settings.IsEnabled, ev.target.value, settings.RecordingDuration, settings.TypingSpeed, settings.KeywordReplacements)]));
    }], (elems = map((tupledArg) => {
        const name = tupledArg[0];
        return createElement("option", {
            value: name,
            children: toText(printf("%s - %s"))(name)(tupledArg[1]),
        });
    }, ofArray([["Tiny", "Fastest, lowest accuracy (~39MB)"], ["Base", "Balanced speed and accuracy (~74MB) - Recommended"], ["Small", "Better accuracy, slower (~244MB)"], ["Medium", "High accuracy, slow (~769MB)"], ["Large", "Best accuracy, very slow (~1550MB)"]])), ["children", Interop_reactApi.Children.toArray(Array.from(elems))])]))), createElement("div", createObj(ofArray([["className", "bg-amber-500/10 border border-amber-500/30 rounded p-4"], (elems_2 = [createElement("p", createObj(ofArray([["className", "text-amber-500 font-medium flex items-center"], (elems_1 = [createElement("span", {
        className: "text-xl mr-2",
        children: "⚠️",
    }), createElement("span", {
        children: ["Requires application restart to take effect"],
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])])))), undefined));
}

function typingSpeedSelector(currentSpeed, dispatch, settings) {
    let elems_1;
    return card(new CardProps("Typing Speed", singleton_1(createElement("div", createObj(ofArray([["className", "space-y-3"], (elems_1 = map((tupledArg) => {
        let value_3, elems, children;
        const value_2 = tupledArg[0];
        return createElement("label", createObj(ofArray([(value_3 = "flex items-start space-x-3 p-3 rounded border border-text-secondary/30 hover:border-primary/50 cursor-pointer transition-colors", ["className", value_3]), (elems = [createElement("input", {
            type: "radio",
            name: "typingSpeed",
            value: value_2,
            checked: currentSpeed.toLocaleLowerCase() === value_2,
            onChange: (ev) => {
                if (ev.target.checked) {
                    dispatch(new Msg(3, [new AppSettings(settings.HotkeyKey, settings.HotkeyModifiers, settings.IsEnabled, settings.ModelSize, settings.RecordingDuration, value_2, settings.KeywordReplacements)]));
                }
            },
            className: "mt-1",
        }), (children = ofArray([createElement("p", {
            className: "font-medium text-text-primary",
            children: tupledArg[1],
        }), createElement("p", {
            className: "text-sm text-text-secondary",
            children: tupledArg[2],
        })]), createElement("div", {
            children: Interop_reactApi.Children.toArray(Array.from(children)),
        }))], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])));
    }, ofArray([["fast", "Fast (5ms delay)", "Best for quick typing"], ["normal", "Normal (10ms delay)", "Recommended for most users"], ["slow", "Slow (20ms delay)", "More reliable for older systems"]])), ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])])))), undefined));
}

export function view(settings, isRecordingHotkey, dispatch) {
    let elems_5;
    return createElement("div", createObj(ofArray([["className", "space-y-6"], (elems_5 = toList(delay(() => append(singleton(createElement("h2", {
        className: "text-3xl font-bold text-text-primary mb-6",
        children: "General Settings",
    })), delay(() => {
        let elems_2, elems_3, elems_4, elems_1, elems;
        const matchValue = settings;
        switch (matchValue.tag) {
            case 1:
                return singleton(createElement("div", createObj(ofArray([["className", "flex items-center justify-center py-12"], (elems_2 = [createElement("div", {
                    className: "animate-spin rounded-full h-12 w-12 border-b-2 border-primary",
                })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])]))));
            case 3:
                return singleton(createElement("div", createObj(ofArray([["className", "bg-red-500/10 border border-red-500/30 rounded p-6"], (elems_3 = [createElement("p", {
                    className: "text-red-500",
                    children: toText(printf("Error loading settings: %s"))(matchValue.fields[0]),
                })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])]))));
            case 0:
                return singleton(createElement("div", createObj(ofArray([["className", "flex items-center justify-center py-12"], (elems_4 = [createElement("div", {
                    className: "animate-spin rounded-full h-12 w-12 border-b-2 border-primary",
                })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_4))])]))));
            default: {
                const s = matchValue.fields[0];
                return singleton(createElement("div", createObj(ofArray([["className", "space-y-6"], (elems_1 = [createElement(hotkeyRecorder, {
                    isRecording: isRecordingHotkey,
                    currentKey: s.HotkeyKey,
                    currentModifiers: s.HotkeyModifiers,
                    dispatch: dispatch,
                }), createElement("div", createObj(ofArray([["className", "grid grid-cols-1 md:grid-cols-2 gap-6"], (elems = [modelSelector(s.ModelSize, dispatch, s), typingSpeedSelector(s.TypingSpeed, dispatch, s)], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])]))));
            }
        }
    })))), ["children", Interop_reactApi.Children.toArray(Array.from(elems_5))])])));
}

