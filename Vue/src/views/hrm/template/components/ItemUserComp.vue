<script>
import { onMounted } from 'vue';
import CusAvartaComponent from './CusAvartaComponent.vue'
export default ({
    components: {
        CusAvartaComponent,
    },
    props: {
        user: Object,
        basic: Boolean,
        size:String
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
    <div class="flex align-items-center" v-if="props.user">
        <CusAvartaComponent :user="props.user" :size="props.size" class="mr-2"></CusAvartaComponent>
        <div>
            <b>{{ props.user.full_name }}</b><br />
            <div v-if="!props.basic">
                <div v-if="props.user.department_name" class="mb-1">
                    <i style="font-size: 85%;">{{
                        props.user.department_name }}</i>
                </div>
                <i style="font-size: 85%;">{{ props.user.organization_name }}</i>
            </div>
        </div>
    </div>
</template>
<style lang="scss" scoped></style>
