import { Record, Union } from "../../fable_modules/fable-library-js.4.25.0/Types.js";
import { record_type, option_type, bool_type, lambda_type, unit_type, string_type, union_type } from "../../fable_modules/fable-library-js.4.25.0/Reflection.js";
import { createElement } from "react";
import { defaultArg } from "../../fable_modules/fable-library-js.4.25.0/Option.js";
import { printf, toText } from "../../fable_modules/fable-library-js.4.25.0/String.js";

export class ButtonVariant extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Primary", "Secondary", "Success", "Danger", "Ghost"];
    }
}

export function ButtonVariant_$reflection() {
    return union_type("Components.Button.ButtonVariant", [], ButtonVariant, () => [[], [], [], [], []]);
}

export class ButtonSize extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Small", "Medium", "Large"];
    }
}

export function ButtonSize_$reflection() {
    return union_type("Components.Button.ButtonSize", [], ButtonSize, () => [[], [], []]);
}

export class ButtonProps extends Record {
    constructor(Text$, OnClick, Variant, Size, Disabled, ClassName) {
        super();
        this.Text = Text$;
        this.OnClick = OnClick;
        this.Variant = Variant;
        this.Size = Size;
        this.Disabled = Disabled;
        this.ClassName = ClassName;
    }
}

export function ButtonProps_$reflection() {
    return record_type("Components.Button.ButtonProps", [], ButtonProps, () => [["Text", string_type], ["OnClick", lambda_type(unit_type, unit_type)], ["Variant", ButtonVariant_$reflection()], ["Size", ButtonSize_$reflection()], ["Disabled", bool_type], ["ClassName", option_type(string_type)]]);
}

function getVariantClasses(variant) {
    switch (variant.tag) {
        case 1:
            return "bg-secondary hover:bg-secondary-600 text-white";
        case 2:
            return "bg-green-500 hover:bg-green-600 text-white";
        case 3:
            return "bg-red-500 hover:bg-red-600 text-white";
        case 4:
            return "bg-transparent hover:bg-white/10 text-text-primary border border-text-secondary";
        default:
            return "bg-primary hover:bg-primary-600 text-white";
    }
}

function getSizeClasses(size) {
    switch (size.tag) {
        case 1:
            return "px-4 py-2 text-base";
        case 2:
            return "px-6 py-3 text-lg";
        default:
            return "px-3 py-1.5 text-sm";
    }
}

export function button(props) {
    let arg, arg_1, arg_2;
    return createElement("button", {
        className: (arg = getVariantClasses(props.Variant), (arg_1 = getSizeClasses(props.Size), (arg_2 = defaultArg(props.ClassName, ""), toText(printf("rounded font-medium transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed %s %s %s"))(arg)(arg_1)(arg_2)))),
        onClick: (_arg) => {
            if (!props.Disabled) {
                props.OnClick();
            }
        },
        disabled: props.Disabled,
        children: props.Text,
    });
}

export function primaryButton(text, onClick) {
    return button(new ButtonProps(text, onClick, new ButtonVariant(0, []), new ButtonSize(1, []), false, undefined));
}

export function secondaryButton(text, onClick) {
    return button(new ButtonProps(text, onClick, new ButtonVariant(1, []), new ButtonSize(1, []), false, undefined));
}

export function dangerButton(text, onClick) {
    return button(new ButtonProps(text, onClick, new ButtonVariant(3, []), new ButtonSize(1, []), false, undefined));
}

