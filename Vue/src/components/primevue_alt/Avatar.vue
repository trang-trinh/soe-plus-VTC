<template>
  <div
    :class="containerClass"
    :aria-labelledby="ariaLabelledby"
    :aria-label="ariaLabel"
  >
    <slot>
      <span
        v-if="label"
        class="p-avatar-text"
        >{{ label }}</span
      >
      <span
        v-else-if="icon"
        :class="iconClass"
      ></span>
      <img
        v-else-if="image"
        :src="image"
        :alt="ariaLabel"
        @error="onError"
      />
    </slot>
  </div>
</template>

<script>
import img from "../../assets/image/nouser1.png";
export default {
  name: "Avatar",
  props: {
    label: {
      type: String,
      default: null,
    },
    icon: {
      type: String,
      default: null,
    },
    image: {
      type: String,
      default: null,
    },
    size: {
      type: String,
      default: "normal",
    },
    shape: {
      type: String,
      default: "square",
    },
    "aria-labelledby": {
      type: String,
      default: null,
    },
    "aria-label": {
      type: String,
      default: null,
    },
  },
  methods: {
    onError(e) {
      e.target.src = img;
    },
  },
  computed: {
    containerClass() {
      return [
        "p-avatar p-component",
        {
          "p-avatar-image": this.image != null,
          "p-avatar-circle": this.shape === "circle",
          "p-avatar-lg": this.size === "large",
          "p-avatar-xl": this.size === "xlarge",
        },
      ];
    },
    iconClass() {
      return ["p-avatar-icon", this.icon];
    },
  },
};
</script>

<style scoped>
.p-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  font-size: 1rem;
}

/*.p-avatar.p-avatar-image {
  background-color: transparent;
}*/

.p-avatar.p-avatar-circle {
  border-radius: 50%;
}

.p-avatar-circle img {
  border-radius: 50%;
}

.p-avatar .p-avatar-icon {
  font-size: 1rem;
}

.p-avatar img {
  width: 100%;
  height: 100%;
}
</style>
