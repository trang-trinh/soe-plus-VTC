<script>
import { onMounted } from 'vue';
import CusAvartaComponent from './CusAvartaComponent.vue'
export default ({
    components: {
        CusAvartaComponent,
    },
    props: {
        users: Array,
        size: String,
        maxitem: Number
    },
    setup(props) {
        const users = props.users?props.users.slice(0, props.maxitem - 1):[];
        onMounted(() => {

        });
        return {
            props,
            users
        }
    },
})
</script>
<template>
    <AvatarGroup v-if="props.users" class="m-auto">
        <CusAvartaComponent v-for="u in users" :user="u" :size="props.size"></CusAvartaComponent>
        <Avatar v-if="(props.users.length - props.maxitem) > 0" :label="'+' + (props.users.length - props.maxitem)" shape="circle"
            :size="props.size" style="background-color:orange, color:#ffffff" />
    </AvatarGroup>
</template>
<style lang="scss" scoped></style>
