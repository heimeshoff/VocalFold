import { Record } from "../../fable_modules/fable-library-js.4.25.0/Types.js";
import { record_type, option_type, list_type, class_type, string_type } from "../../fable_modules/fable-library-js.4.25.0/Reflection.js";
import { createElement } from "react";
import { createObj } from "../../fable_modules/fable-library-js.4.25.0/Util.js";
import { value as value_15, defaultArg } from "../../fable_modules/fable-library-js.4.25.0/Option.js";
import { printf, toText } from "../../fable_modules/fable-library-js.4.25.0/String.js";
import { Interop_reactApi } from "../../fable_modules/Feliz.2.7.0/Interop.fs.js";
import { ofArray } from "../../fable_modules/fable-library-js.4.25.0/List.js";
import { empty, singleton, append, delay, toList } from "../../fable_modules/fable-library-js.4.25.0/Seq.js";
import { defaultOf } from "../../fable_modules/fable-library-js.4.25.0/Util.js";

export class CardProps extends Record {
    constructor(Title, Children, ClassName) {
        super();
        this.Title = Title;
        this.Children = Children;
        this.ClassName = ClassName;
    }
}

export function CardProps_$reflection() {
    return record_type("Components.Card.CardProps", [], CardProps, () => [["Title", string_type], ["Children", list_type(class_type("Fable.React.ReactElement"))], ["ClassName", option_type(string_type)]]);
}

export function card(props) {
    let arg, elems_1;
    return createElement("div", createObj(ofArray([["className", (arg = defaultArg(props.ClassName, ""), toText(printf("bg-background-card rounded-lg shadow-lg p-6 %s"))(arg))], (elems_1 = [createElement("h3", {
        className: "text-xl font-semibold mb-4 text-text-primary",
        children: props.Title,
    }), createElement("div", {
        className: "space-y-3",
        children: Interop_reactApi.Children.toArray(Array.from(props.Children)),
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])])));
}

export function statusCard(title, value, color, icon) {
    let elems_1, elems;
    return createElement("div", createObj(ofArray([["className", "bg-background-card rounded-lg shadow-lg p-6 hover:shadow-xl transition-all duration-200"], (elems_1 = [createElement("div", createObj(ofArray([["className", "flex items-center justify-between"], (elems = toList(delay(() => {
        let children;
        return append(singleton((children = ofArray([createElement("h3", {
            className: "text-sm font-medium text-text-secondary mb-1",
            children: title,
        }), createElement("p", {
            className: toText(printf("text-2xl font-bold %s"))(color),
            children: value,
        })]), createElement("div", {
            children: Interop_reactApi.Children.toArray(Array.from(children)),
        }))), delay(() => {
            const matchValue = icon;
            if (matchValue == null) {
                return singleton(defaultOf());
            }
            else {
                value_15(matchValue);
                return empty();
            }
        }));
    })), ["children", Interop_reactApi.Children.toArray(Array.from(elems))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])])));
}

