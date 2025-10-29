import { createElement } from "react";
import { createObj } from "../../fable_modules/fable-library-js.4.25.0/Util.js";
import { printf, toText } from "../../fable_modules/fable-library-js.4.25.0/String.js";
import { Interop_reactApi } from "../../fable_modules/Feliz.2.7.0/./Interop.fs.js";
import { singleton as singleton_1, length, ofArray } from "../../fable_modules/fable-library-js.4.25.0/List.js";
import { singleton, append, delay, toList } from "../../fable_modules/fable-library-js.4.25.0/Seq.js";
import { Page, Msg } from "../Types.fs.js";
import { CardProps, card } from "./Card.fs.js";
import { defaultOf } from "../../fable_modules/Feliz.2.7.0/../fable-library-js.4.25.0/Util.js";

function statusIndicator(isEnabled) {
    let elems, arg;
    return createElement("div", createObj(ofArray([["className", "flex items-center space-x-2"], (elems = [createElement("div", {
        className: (arg = (isEnabled ? "bg-green-500 animate-pulse" : "bg-gray-500"), toText(printf("w-3 h-3 rounded-full %s"))(arg)),
    }), createElement("span", {
        className: "text-sm font-medium",
        children: isEnabled ? "Active" : "Inactive",
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])));
}

function statusCard(status, dispatch) {
    let elems_5, elems_4, elems, elems_3, elems_1, elems_2;
    return createElement("div", createObj(ofArray([["className", "bg-gradient-to-br from-primary/20 to-secondary/20 rounded-lg shadow-lg p-6 border border-primary/30"], (elems_5 = [createElement("h3", {
        className: "text-xl font-semibold mb-4 text-text-primary",
        children: "Status",
    }), createElement("div", createObj(ofArray([["className", "space-y-4"], (elems_4 = [createElement("div", createObj(ofArray([["className", "flex items-center justify-between"], (elems = toList(delay(() => append(singleton(statusIndicator(status.IsEnabled)), delay(() => {
        let arg;
        return singleton(createElement("button", {
            className: (arg = (status.IsEnabled ? "bg-red-500 hover:bg-red-600 text-white" : "bg-green-500 hover:bg-green-600 text-white"), toText(printf("px-6 py-2 rounded-lg font-medium transition-all duration-200 %s"))(arg)),
            children: status.IsEnabled ? "Disable" : "Enable",
            onClick: (_arg) => {
                dispatch(new Msg(19, []));
            },
        }));
    })))), ["children", Interop_reactApi.Children.toArray(Array.from(elems))])]))), createElement("div", createObj(ofArray([["className", "pt-4 border-t border-white/10"], (elems_3 = [createElement("div", createObj(ofArray([["className", "flex items-center justify-between mb-2"], (elems_1 = [createElement("span", {
        className: "text-text-secondary text-sm",
        children: "Global Hotkey",
    }), createElement("span", {
        className: "text-primary font-mono font-semibold",
        children: status.CurrentHotkey,
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])]))), createElement("div", createObj(ofArray([["className", "flex items-center justify-between"], (elems_2 = [createElement("span", {
        className: "text-text-secondary text-sm",
        children: "Version",
    }), createElement("span", {
        className: "text-text-primary font-semibold",
        children: status.Version,
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_4))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_5))])])));
}

function quickStatsCard(settings) {
    let elems, children, children_2, children_4, arg, children_6;
    return card(new CardProps("Quick Stats", singleton_1(createElement("div", createObj(ofArray([["className", "grid grid-cols-2 gap-4"], (elems = [(children = ofArray([createElement("p", {
        className: "text-text-secondary text-sm",
        children: "Model",
    }), createElement("p", {
        className: "text-primary font-semibold text-lg",
        children: settings.ModelSize,
    })]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children)),
    })), (children_2 = ofArray([createElement("p", {
        className: "text-text-secondary text-sm",
        children: "Typing Speed",
    }), createElement("p", {
        className: "text-secondary font-semibold text-lg",
        children: settings.TypingSpeed.toLocaleUpperCase(),
    })]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children_2)),
    })), (children_4 = ofArray([createElement("p", {
        className: "text-text-secondary text-sm",
        children: "Keywords",
    }), createElement("p", {
        className: "text-primary font-semibold text-lg",
        children: (arg = (length(settings.KeywordReplacements) | 0), toText(printf("%d configured"))(arg)),
    })]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children_4)),
    })), (children_6 = ofArray([createElement("p", {
        className: "text-text-secondary text-sm",
        children: "Recording Duration",
    }), createElement("p", {
        className: "text-secondary font-semibold text-lg",
        children: (settings.RecordingDuration === 0) ? "Hold to speak" : toText(printf("%ds"))(settings.RecordingDuration),
    })]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children_6)),
    }))], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))), undefined));
}

function quickActionsCard(dispatch) {
    let elems, value_2, value_7;
    return card(new CardProps("Quick Actions", singleton_1(createElement("div", createObj(ofArray([["className", "grid grid-cols-2 gap-3"], (elems = [createElement("button", createObj(ofArray([(value_2 = "px-4 py-3 bg-primary hover:bg-primary-600 text-white rounded-lg font-medium transition-all duration-200 hover:shadow-lg", ["className", value_2]), ["children", "Configure Hotkey"], ["onClick", (_arg) => {
        dispatch(new Msg(0, [new Page(1, [])]));
    }]]))), createElement("button", createObj(ofArray([(value_7 = "px-4 py-3 bg-secondary hover:bg-secondary-600 text-white rounded-lg font-medium transition-all duration-200 hover:shadow-lg", ["className", value_7]), ["children", "Manage Keywords"], ["onClick", (_arg_1) => {
        dispatch(new Msg(0, [new Page(2, [])]));
    }]])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))), undefined));
}

function loadingCard(title) {
    let elems;
    return card(new CardProps(title, singleton_1(createElement("div", createObj(ofArray([["className", "flex items-center justify-center py-8"], (elems = [createElement("div", {
        className: "animate-spin rounded-full h-8 w-8 border-b-2 border-primary",
    }), createElement("span", {
        className: "ml-3 text-text-secondary",
        children: "Loading...",
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))), undefined));
}

function errorCard(title, error) {
    let elems;
    return card(new CardProps(title, singleton_1(createElement("div", createObj(ofArray([["className", "bg-red-500/10 border border-red-500/30 rounded p-4"], (elems = [createElement("p", {
        className: "text-red-500 text-sm",
        children: toText(printf("Error: %s"))(error),
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))), undefined));
}

export function view(status, settings, dispatch) {
    let elems_3;
    return createElement("div", createObj(ofArray([["className", "space-y-6"], (elems_3 = toList(delay(() => append(singleton(createElement("h2", {
        className: "text-3xl font-bold text-text-primary mb-6",
        children: "Dashboard",
    })), delay(() => {
        let matchValue;
        return append((matchValue = status, (matchValue.tag === 1) ? singleton(loadingCard("Status")) : ((matchValue.tag === 3) ? singleton(errorCard("Status", matchValue.fields[0])) : ((matchValue.tag === 0) ? singleton(defaultOf()) : singleton(statusCard(matchValue.fields[0], dispatch))))), delay(() => {
            let elems;
            return append(singleton(createElement("div", createObj(ofArray([["className", "grid grid-cols-1 md:grid-cols-2 gap-6"], (elems = toList(delay(() => {
                let matchValue_1;
                return append((matchValue_1 = settings, (matchValue_1.tag === 1) ? singleton(loadingCard("Quick Stats")) : ((matchValue_1.tag === 3) ? singleton(errorCard("Quick Stats", matchValue_1.fields[0])) : ((matchValue_1.tag === 0) ? singleton(defaultOf()) : singleton(quickStatsCard(matchValue_1.fields[0]))))), delay(() => singleton(quickActionsCard(dispatch))));
            })), ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))), delay(() => {
                let elems_2, elems_1, children, value_23;
                return singleton(createElement("div", createObj(ofArray([["className", "bg-primary/10 border border-primary/30 rounded-lg p-4"], (elems_2 = [createElement("div", createObj(ofArray([["className", "flex items-start space-x-3"], (elems_1 = [createElement("div", {
                    className: "text-primary text-xl",
                    children: "ℹ️",
                }), (children = ofArray([createElement("p", {
                    className: "text-text-primary font-medium mb-1",
                    children: "How to use VocalFold",
                }), createElement("p", createObj(ofArray([["className", "text-text-secondary text-sm"], (value_23 = "Press your global hotkey to start recording. Speak your text, and it will be typed automatically at your cursor position. Use keywords to quickly insert frequently used text.", ["children", value_23])])))]), createElement("div", {
                    children: Interop_reactApi.Children.toArray(Array.from(children)),
                }))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])]))));
            }));
        }));
    })))), ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])])));
}

