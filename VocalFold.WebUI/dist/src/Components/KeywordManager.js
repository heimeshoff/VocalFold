import { useFeliz_React__React_useState_Static_1505 } from "../../fable_modules/Feliz.2.7.0/React.fs.js";
import { map, defaultArg } from "../../fable_modules/fable-library-js.4.25.0/Option.js";
import { substring, replace, printf, toText, isNullOrWhiteSpace } from "../../fable_modules/fable-library-js.4.25.0/String.js";
import { createElement } from "react";
import { equals, createObj } from "../../fable_modules/fable-library-js.4.25.0/Util.js";
import { Interop_reactApi } from "../../fable_modules/Feliz.2.7.0/./Interop.fs.js";
import { length, item, singleton as singleton_1, mapIndexed, isEmpty, ofArray } from "../../fable_modules/fable-library-js.4.25.0/List.js";
import { empty, singleton, append, delay, toList } from "../../fable_modules/fable-library-js.4.25.0/Seq.js";
import { Msg, KeywordReplacement } from "../Types.fs.js";
import { defaultOf } from "../../fable_modules/Feliz.2.7.0/../fable-library-js.4.25.0/Util.js";

function keywordModal(keyword, index, dispatch, onClose) {
    let elems_5, elems_4, elems, elems_2, children, value_27, children_2, value_40, elems_1, value_53, elems_3;
    const patternInput = useFeliz_React__React_useState_Static_1505(defaultArg(map((k) => k.Keyword, keyword), ""));
    const keywordInput = patternInput[0];
    const patternInput_1 = useFeliz_React__React_useState_Static_1505(defaultArg(map((k_1) => k_1.Replacement, keyword), ""));
    const replacementInput = patternInput_1[0];
    const isValid = !isNullOrWhiteSpace(keywordInput);
    return createElement("div", createObj(ofArray([["className", "fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50"], ["onClick", (e) => {
        if (equals(e.target, e.currentTarget)) {
            onClose();
        }
    }], (elems_5 = [createElement("div", createObj(ofArray([["className", "bg-background-card rounded-lg shadow-2xl w-full max-w-2xl mx-4 animate-slide-up"], ["onClick", (e_1) => {
        e_1.stopPropagation();
    }], (elems_4 = [createElement("div", createObj(ofArray([["className", "flex items-center justify-between p-6 border-b border-white/10"], (elems = [createElement("h3", {
        className: "text-2xl font-bold text-text-primary",
        children: (index != null) ? "Edit Keyword" : "Add Keyword",
    }), createElement("button", {
        className: "text-text-secondary hover:text-text-primary transition-colors text-2xl",
        children: "×",
        onClick: (_arg) => {
            onClose();
        },
    })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))])]))), createElement("div", createObj(ofArray([["className", "p-6 space-y-4"], (elems_2 = [(children = ofArray([createElement("label", {
        className: "block text-sm font-medium text-text-primary mb-2",
        children: "Keyword (what you say)",
    }), createElement("input", createObj(ofArray([["type", "text"], (value_27 = "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors", ["className", value_27]), ["placeholder", "e.g., \'comma\', \'German email footer\'"], ["value", keywordInput], ["onChange", (ev) => {
        patternInput[1](ev.target.value);
    }], ["autoFocus", true]])))]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children)),
    })), (children_2 = ofArray([createElement("label", {
        className: "block text-sm font-medium text-text-primary mb-2",
        children: "Replacement (what to type)",
    }), createElement("textarea", createObj(ofArray([(value_40 = "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors font-mono", ["className", value_40]), ["placeholder", "e.g., \',\', \'Best regards,\\nJohn Doe\'"], ["value", replacementInput], ["onChange", (ev_1) => {
        patternInput_1[1](ev_1.target.value);
    }], ["rows", 4]])))]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children_2)),
    })), createElement("div", createObj(ofArray([["className", "bg-primary/10 border border-primary/30 rounded p-3"], (elems_1 = [createElement("p", createObj(ofArray([["className", "text-sm text-text-secondary"], (value_53 = "Tip: Keywords are matched case-insensitively and as whole phrases. Use \\n for newlines in replacement text.", ["children", value_53])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])]))), createElement("div", createObj(ofArray([["className", "flex items-center justify-end space-x-3 p-6 border-t border-white/10"], (elems_3 = toList(delay(() => append(singleton(createElement("button", {
        className: "px-6 py-2 bg-transparent hover:bg-white/10 text-text-primary rounded-lg font-medium transition-all",
        children: "Cancel",
        onClick: (_arg_1) => {
            onClose();
        },
    })), delay(() => {
        let arg;
        return singleton(createElement("button", {
            className: (arg = (isValid ? "bg-primary hover:bg-primary-600 text-white" : "bg-gray-500 text-gray-300 cursor-not-allowed"), toText(printf("px-6 py-2 rounded-lg font-medium transition-all %s"))(arg)),
            children: "Save",
            disabled: !isValid,
            onClick: (_arg_2) => {
                if (isValid) {
                    dispatch(new Msg(15, [new KeywordReplacement(keywordInput.trim(), replacementInput)]));
                    onClose();
                }
            },
        }));
    })))), ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_4))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_5))])])));
}

function keywordTable(keywords, dispatch, onEdit) {
    let elems_1, value_14, elems, elems_10, elems_9, elems_2, children, elems_8;
    if (isEmpty(keywords)) {
        return createElement("div", createObj(ofArray([["className", "bg-background-card rounded-lg p-12 text-center"], (elems_1 = [createElement("div", {
            className: "text-6xl mb-4",
            children: "📝",
        }), createElement("h3", {
            className: "text-xl font-semibold text-text-primary mb-2",
            children: "No keywords configured",
        }), createElement("p", {
            className: "text-text-secondary mb-6",
            children: "Add your first keyword to get started. Keywords let you quickly insert frequently used text.",
        }), createElement("button", createObj(ofArray([(value_14 = "px-6 py-2 bg-primary hover:bg-primary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2", ["className", value_14]), (elems = [createElement("span", {
            children: ["+"],
        }), createElement("span", {
            children: ["Add Keyword"],
        })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))]), ["onClick", (_arg) => {
            onEdit(-1);
        }]])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))])])));
    }
    else {
        return createElement("div", createObj(ofArray([["className", "bg-background-card rounded-lg overflow-hidden"], (elems_10 = [createElement("table", createObj(ofArray([["className", "w-full"], (elems_9 = [createElement("thead", createObj(ofArray([["className", "bg-background-sidebar"], (elems_2 = [(children = ofArray([createElement("th", {
            className: "text-left px-6 py-3 text-sm font-semibold text-text-primary",
            children: "Keyword",
        }), createElement("th", {
            className: "text-left px-6 py-3 text-sm font-semibold text-text-primary",
            children: "Replacement",
        }), createElement("th", {
            className: "text-right px-6 py-3 text-sm font-semibold text-text-primary",
            children: "Actions",
        })]), createElement("tr", {
            children: Interop_reactApi.Children.toArray(Array.from(children)),
        }))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])]))), createElement("tbody", createObj(singleton_1((elems_8 = mapIndexed((i, keyword) => {
            let elems_7, elems_3, elems_4, text, elems_6, elems_5;
            return createElement("tr", createObj(ofArray([["key", toText(printf("keyword-%d"))(i)], ["className", "border-t border-white/10 hover:bg-white/5 transition-colors"], (elems_7 = [createElement("td", createObj(ofArray([["className", "px-6 py-4"], (elems_3 = [createElement("span", {
                className: "font-mono text-primary font-medium",
                children: keyword.Keyword,
            })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])]))), createElement("td", createObj(ofArray([["className", "px-6 py-4"], (elems_4 = [createElement("span", {
                className: "font-mono text-text-secondary text-sm",
                children: (text = replace(keyword.Replacement, "\n", "\\n"), (text.length > 50) ? (substring(text, 0, 47) + "...") : text),
            })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_4))])]))), createElement("td", createObj(ofArray([["className", "px-6 py-4 text-right"], (elems_6 = [createElement("div", createObj(ofArray([["className", "inline-flex space-x-2"], (elems_5 = [createElement("button", {
                className: "px-3 py-1 bg-primary/20 hover:bg-primary/30 text-primary rounded font-medium text-sm transition-all",
                children: "Edit",
                onClick: (_arg_1) => {
                    dispatch(new Msg(13, [i]));
                    onEdit(i);
                },
            }), createElement("button", {
                className: "px-3 py-1 bg-red-500/20 hover:bg-red-500/30 text-red-500 rounded font-medium text-sm transition-all",
                children: "Delete",
                onClick: (_arg_2) => {
                    dispatch(new Msg(14, [i]));
                },
            })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_5))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_6))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_7))])])));
        }, keywords), ["children", Interop_reactApi.Children.toArray(Array.from(elems_8))]))))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_9))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_10))])])));
    }
}

export function view(settings, editingKeyword, dispatch) {
    let elems_8;
    const patternInput = useFeliz_React__React_useState_Static_1505(false);
    const setShowModal = patternInput[1];
    const patternInput_1 = useFeliz_React__React_useState_Static_1505(undefined);
    const setEditIndex = patternInput_1[1];
    const patternInput_2 = useFeliz_React__React_useState_Static_1505(undefined);
    const setEditKeyword = patternInput_2[1];
    const handleEdit = (index) => {
        let matchResult, s_1;
        if (settings.tag === 2) {
            if ((index >= 0) && (index < length(settings.fields[0].KeywordReplacements))) {
                matchResult = 0;
                s_1 = settings.fields[0];
            }
            else if (index === -1) {
                matchResult = 1;
            }
            else {
                matchResult = 2;
            }
        }
        else {
            matchResult = 2;
        }
        switch (matchResult) {
            case 0: {
                setEditIndex(index);
                setEditKeyword(item(index, s_1.KeywordReplacements));
                setShowModal(true);
                break;
            }
            case 1: {
                setEditIndex(undefined);
                setEditKeyword(undefined);
                setShowModal(true);
                break;
            }
            case 2: {
                break;
            }
        }
    };
    return createElement("div", createObj(ofArray([["className", "space-y-6"], (elems_8 = toList(delay(() => {
        let elems_3;
        return append(singleton(createElement("div", createObj(ofArray([["className", "flex items-center justify-between"], (elems_3 = toList(delay(() => {
            let children;
            return append(singleton((children = ofArray([createElement("h2", {
                className: "text-3xl font-bold text-text-primary mb-2",
                children: "Keyword Replacements",
            }), createElement("p", {
                className: "text-text-secondary",
                children: "Configure keywords that will be automatically replaced in transcriptions",
            })]), createElement("div", {
                children: Interop_reactApi.Children.toArray(Array.from(children)),
            }))), delay(() => {
                let elems_2, value_14, elems, value_20, elems_1;
                const matchValue = settings;
                let matchResult_1, s_3;
                if (matchValue.tag === 2) {
                    if (!isEmpty(matchValue.fields[0].KeywordReplacements)) {
                        matchResult_1 = 0;
                        s_3 = matchValue.fields[0];
                    }
                    else {
                        matchResult_1 = 1;
                    }
                }
                else {
                    matchResult_1 = 1;
                }
                switch (matchResult_1) {
                    case 0:
                        return singleton(createElement("div", createObj(ofArray([["className", "flex space-x-3"], (elems_2 = [createElement("button", createObj(ofArray([(value_14 = "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2", ["className", value_14]), (elems = [createElement("span", {
                            children: ["✨"],
                        }), createElement("span", {
                            children: ["Add Examples"],
                        })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))]), ["onClick", (_arg) => {
                            dispatch(new Msg(17, []));
                        }]]))), createElement("button", createObj(ofArray([(value_20 = "px-4 py-2 bg-primary hover:bg-primary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2", ["className", value_20]), (elems_1 = [createElement("span", {
                            children: ["+"],
                        }), createElement("span", {
                            children: ["Add Keyword"],
                        })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_1))]), ["onClick", (_arg_1) => {
                            handleEdit(-1);
                        }]])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_2))])]))));
                    default:
                        return singleton(defaultOf());
                }
            }));
        })), ["children", Interop_reactApi.Children.toArray(Array.from(elems_3))])])))), delay(() => {
            let elems_5, elems_4, children_2, value_42;
            return append(singleton(createElement("div", createObj(ofArray([["className", "bg-primary/10 border border-primary/30 rounded-lg p-4"], (elems_5 = [createElement("div", createObj(ofArray([["className", "flex items-start space-x-3"], (elems_4 = [createElement("div", {
                className: "text-primary text-xl",
                children: "ℹ️",
            }), (children_2 = ofArray([createElement("p", {
                className: "text-text-primary font-medium mb-1",
                children: "How keyword replacements work",
            }), createElement("p", createObj(ofArray([["className", "text-text-secondary text-sm"], (value_42 = "When you speak a keyword, it will be automatically replaced with the configured text. For example, saying \'comma\' can be replaced with \',\'. Keywords are matched case-insensitively.", ["children", value_42])])))]), createElement("div", {
                children: Interop_reactApi.Children.toArray(Array.from(children_2)),
            }))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_4))])])))], ["children", Interop_reactApi.Children.toArray(Array.from(elems_5))])])))), delay(() => {
                let matchValue_1, elems_6, elems_7;
                return append((matchValue_1 = settings, (matchValue_1.tag === 1) ? singleton(createElement("div", createObj(ofArray([["className", "flex items-center justify-center py-12"], (elems_6 = [createElement("div", {
                    className: "animate-spin rounded-full h-12 w-12 border-b-2 border-primary",
                })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_6))])])))) : ((matchValue_1.tag === 3) ? singleton(createElement("div", createObj(ofArray([["className", "bg-red-500/10 border border-red-500/30 rounded p-6"], (elems_7 = [createElement("p", {
                    className: "text-red-500",
                    children: toText(printf("Error loading keywords: %s"))(matchValue_1.fields[0]),
                })], ["children", Interop_reactApi.Children.toArray(Array.from(elems_7))])])))) : ((matchValue_1.tag === 0) ? singleton(defaultOf()) : singleton(keywordTable(matchValue_1.fields[0].KeywordReplacements, dispatch, handleEdit))))), delay(() => (patternInput[0] ? singleton(keywordModal(patternInput_2[0], patternInput_1[0], dispatch, () => {
                    setShowModal(false);
                    setEditIndex(undefined);
                    setEditKeyword(undefined);
                })) : empty())));
            }));
        }));
    })), ["children", Interop_reactApi.Children.toArray(Array.from(elems_8))])])));
}

