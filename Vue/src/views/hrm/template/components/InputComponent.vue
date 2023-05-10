<script>
import { ref, onMounted } from 'vue';
export default ({
    props: {
        ipmodel: Object,
        col: Object,
        okey: String,
        div: Boolean
    },
    setup(props) {
        onMounted(() => {
            autoType();
        });
        const autoType = () => {
            let vl = props.ipmodel[props.okey];
            if (!props.col.inputtype) {
                if (vl && !isNaN(vl)) {
                    if (!props.okey.toString().toLowerCase().includes("năm")) {
                        props.col.inputtype = "Currency";
                    } else {
                        props.col.inputtype = "Number";
                    }
                }
            }
        }
        const textalign = ref("text-left");
        const bindValue = () => {
            let vl = props.ipmodel[props.okey];
            if (props.col.inputtype == "Currency") {
                vl = parseFloat(vl).toLocaleString();
                textalign.value = "text-right";
            }
            if (vl == "NaN") return null;
            return vl;
        }
        const changeDiv = (ev, f) => {
            let input = ev.target;
            let vl = input.textContent;
            if (textalign.value == "text-right") {
                vl = vl.replaceAll(",", "");
            }
            if (f == false)
                props.ipmodel[props.okey] = vl;
            else if (vl && !isNaN(vl) && props.col.inputtype == "Currency") {
                vl = parseFloat(vl).toLocaleString();
                input.textContent = vl;
                input.focus();
                document.execCommand('selectAll', false, null);
                document.getSelection().collapseToEnd();
            }
        }
        const onDateInput = (event) => {
            // Remove non-numeric characters from the input
            const cleanedInput = event.target.value.replace(/\D/g, '');
            // Format the input as a date (DD/MM/YYYY)
            if (cleanedInput.length <= 2) {
                event.target.value = cleanedInput;
            } else if (cleanedInput.length <= 4) {
                event.target.value = cleanedInput.slice(0, 2) + '/' + cleanedInput.slice(2);
            } else {
                event.target.value = cleanedInput.slice(0, 2) + '/' + cleanedInput.slice(2, 4) + '/' + cleanedInput.slice(4, 8);
            }
        }
        const changeDrForm = (ev, r) => {
            if (ev.value.key)
                r.key = ev.value.key;
        }
        return {
            props,
            changeDiv,
            textalign,
            bindValue,
            onDateInput,
            changeDrForm
        }
    },
})
</script>
<template>
    <div v-if="props.div" @keyup="changeDiv($event, true)" @blur="changeDiv($event, false)" contenteditable="true"
        spellcheck="false" :class="textalign + ' w-full'">{{ bindValue() }}</div>
    <div v-else>
        <InputText spellcheck="false" type="number" v-if="props.col.inputtype == 'Number'"
            v-model="props.ipmodel[props.okey]" class="w-full" />
        <InputNumber spellcheck="false" :useGrouping="true" v-if="props.col.inputtype == 'Currency'"
            v-model="props.ipmodel[props.okey]" class="w-full" />
        <Calendar @input="onDateInput" :showOnFocus="false" spellcheck="false" :mask="99 / 99 / 9999"
            v-if="props.col.inputtype == 'Date'" v-model="props.ipmodel[props.okey]" class="w-full" dateFormat="dd/mm/yy"
            showButtonBar showIcon />
        <InputMask spellcheck="false" mask="99/99/9999 99:99" v-if="props.col.inputtype == 'DateTime'"
            v-model="props.ipmodel[props.okey]" class="w-full" />
        <Dropdown spellcheck="false" @change="changeDrForm($event, props.ipmodel)" :editable="true"
            v-if="(!props.col.inputtype || props.col.inputtype == 'Text') && props.col.auto"
            v-model="props.ipmodel[props.okey]" optionLabel="label" :options="itemtypeInputs" optionGroupLabel="label"
            optionGroupChildren="items" :filter="true" :virtualScrollerOptions="{ itemSize: 38 }" class="w-full">
        </Dropdown>
        <Textarea v-if="(!props.col.inputtype || props.col.inputtype == 'Text') && !props.col.auto"
            style="height: auto;padding: 5px;" spellcheck="false" autoResize v-model="props.ipmodel[props.okey]"
            class="w-full"></Textarea>
    </div>
</template>
<!-- <style lang="scss" scoped></style> -->
