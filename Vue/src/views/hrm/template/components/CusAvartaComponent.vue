<script>
import { onMounted } from 'vue';
export default ({
    props: {
        user: Object,
        size: String,
        shape: String
    },
    setup(props) {
        const bgColor = [
            "#F8E69A",
            "#AFDFCF",
            "#F4B2A3",
            "#9A97EC",
            "#CAE2B0",
            "#8BCFFB",
            "#CCADD7",
        ];
        const errorimg = (imgstr) => {
            document.querySelectorAll(`img[src='${imgstr}']`).forEach(img => {
                img.src = "https://project.soe.vn/favicon.ico";
            });
        };
        const compuFirstLetter = (s) => {
            if(s && s.length>0)
            return s.split(" ")[s.split(" ").length - 1].substring(0, 1);
            return "A";
        };
        const baseUrl = fileURL;
        onMounted(() => {
        });
        return {
            props,
            baseUrl,
            compuFirstLetter,
            bgColor,
            errorimg
        }
    },
})
</script>
<template>
    <Avatar @error="errorimg(baseUrl + props.user.avatar)" v-if="props.user.avatar" :image="baseUrl + props.user.avatar"
        :size="props.size" :shape="props.shape || 'circle'" />
    <Avatar v-else :label="compuFirstLetter(props.user.full_name)" :size="props.size" :shape="props.shape || 'circle'" :style="{
        background: bgColor[props.user.is_order % 7],
        color: '#ffffff',
    }" />
</template>
<style lang="scss" scoped></style>
