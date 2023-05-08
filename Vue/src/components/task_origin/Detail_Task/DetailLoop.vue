<script setup>
import DetailedChild from "../DetailedWork.vue";
const props = defineProps({
  keys: Intl,
  id: String,
  turn: Intl,
  closeChildDetail: Function,
});
</script>
<template>
  <div>
    <DetailedChild
      :keys="props.keys"
      :id="props.id"
      :turn="props.turn"
      :closeChildDetail="props.closeChildDetail"
    >
    </DetailedChild>
  </div>
</template>

<style lang="scss" scoped></style>
