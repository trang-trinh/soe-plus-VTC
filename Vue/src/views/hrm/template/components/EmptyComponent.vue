<script>
import { onMounted } from 'vue';
export default ({
    props: {
        message: String
    },
    setup(props) {
        onMounted(() => {
        });
        return {
            props,
        }
    },
})
</script>
<template>
    <div class="text-center p-4">
        {{ props.message || "Chưa có bản ghi nào" }}
    </div>
</template>
<!-- <style lang="scss" scoped></style> -->
