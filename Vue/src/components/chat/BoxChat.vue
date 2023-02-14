<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import { VuemojiPicker } from "vuemoji-picker";
import { forEach } from "jszip";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const socket = inject("socket");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const props = defineProps({
	key: Number,
	detailChat: Object,
	listMessage: Object,
	listMember: Object,
	funcCallSocket: Function,
	funcCallUpdate: Function,
});
const formatByte = ((bytes, precision) => {
	if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
	if (typeof precision === 'undefined') precision = 1;
	let units = ['bytes', 'KB', 'MB', 'GB', 'TB', 'PB'];
	if (typeof bytes === 'string' || bytes instanceof String){
		bytes = parseFloat(bytes);
	}
	let	number = Math.floor(Math.log(bytes) / Math.log(1024));
	return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + ' ' + units[number];
});
const bgColor = ref([
    "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
const goBottomChat = (type) => {
	setTimeout(() => {
		let elmnt = document.getElementById("chat_final");
		if (elmnt != null) {
  			//elmnt.scrollIntoView({ behavior: "smooth" });
  			elmnt.scrollIntoView();
		}
	}, 200);
};
const FileAttach = ref([]);
const noiDungChat = ref({
	noiDung: "",
});

</script>
<template>
	<div class="bg-white">
		<div id="taskmessagepanel" class="scroll-outer" 
			style="min-height: calc(100vh - 200px); max-height: calc(100vh - 200px); background-color: #fff; overflow-y: auto; overflow-x: hidden; " 
			buffered-scroll-up="loadMore(props.listMessage)">
			<div class="scroll-inner scroll-inner-chat" v-if="!IsCall">
				<div class="div-content-chat">
					<div class="task-comment w-full" id="task-comment">
						<div v-for="(u, index) in props.listMessage" :id="index == props.listMessage.length - 1 ? 'chat_final' : ('chat_' + index)" :key="index" ref="index">
							<div class="message-feed media noidungchat" 
								style="overflow: -webkit-paged-y;display: inline-block;width: 100%;float: left;"
								v-if="index == 0 || props.listMessage[index-1].strCreateDate != u.strCreateDate">
								<span class="borderchat"></span>
								<span class="spandatechat">
									{{ u.strCreateDate }}
								</span>
							</div>
							<div v-if="u.messtitle" class="message-feed media noidungchat" style="overflow: -webkit-paged-y;display: inline-block;width: 100%;float: left">											
								<a :href="f.content_message" target="_blank" v-if="u.content_message.includes('http://%') || u.content_message.includes('https://%')">
									<span v-html="f.content_message"></span>
								</a>
								<span v-else v-html="u.content_message" class="spandatechat mt-1" style="width: max-content; background-color: transparent; color: #aaa; max-width: 300px; white-space: normal; overflow: hidden; text-overflow: ellipsis; "></span>
								<span class="borderchat" style="width:0"></span>
							</div>
							<div v-if="!u.messtitle" class="row-comment" style="padding: 15px 0px 8px 0px; background-color: #fff;" 
								:class="{ 'pd-loopfirst': u.loopmefirst, 'pd-loop': u.loopme, 'pd-looplast': u.loopmelast,'pd-left' : u.IsMe, 'pd-right' : !u.IsMe, 'isGroup': props.detailChat.is_group_chat }">
								<div v-if="!u.IsMe" class="r-ava format_center" style="height: max-content;" :class="{ 'loopme': u.loopme }">
									<Avatar
										:image="
											u.user_create_avatar
											? basedomainURL + u.user_create_avatar
											: basedomainURL + '/Portals/Image/noimg.jpg'
										"
										size="large"
										shape="circle"
										v-bind:label="u.user_create_avatar ? '' : (u.last_name ?? '').substring(0, 1)"
										style="cursor: pointer;border: 1px solid #e9e9e9;"
										:style="{ background: (props.detailChat.is_group_chat ? bgColor[index % 7] : bgColor[0]) + '!important'}"
									/>
								</div>
								<div class="r-cbox m-0" 
									style="position: relative;max-width: 80%;"
									:style="(!(u.files != null && u.files.length > 0) || u.IsEdit) ? 'padding: 0.5rem 0.5rem 1rem;' + (u.IsMe ? 'background: #DBF1FF;' : '') :  (u.IsMe ? 'background: #DBF1FF;' : '')"
								>
									<div class="r-cname mx-1" style="color: #aaa;">
										<span v-if="(props.detailChat.is_group_chat && !u.IsMe && !(u.files != null && u.files.length > 0))">{{u.full_name}}</span>
									</div>
									<div v-if="u.type_message == 1 && (u.files != null && u.files.length > 0)" class="r-cm p-0 m-0">
										<div class="image_preview_chat" style="max-width: 300px;" v-for="(imageFile, idxImg) in u.files" :key="idxImg">
											<Image v-if="imageFile.is_image == '1'"
												class="flex"
												:src="basedomainURL + (imageFile.file_path ||'/Portals/Image/noimg.jpg')"
												style="width: 100%; height: 100%; object-fit: contain;"
												preview
											/>
											<div class="pt-1 pb-3 123" v-if="imageFile.is_image == '1'">
												<span class="r-cdate fw-400">
													{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
												</span>
											</div>
										</div>
									</div>
									<div v-if="u.content_message" class="r-cm p-0 mt-0" 
										style="word-break:break-word; margin-bottom: 0px;border-radius: 20px;width: calc(100% - 1.25rem);"
										:style="u.IsMe ? 'background: #DBF1FF;' : ''">
										<div class="reply-chat show-reply" v-if="u.ParentComment" style="padding: 10px;border-bottom: 0.5px solid #ccc;">
											<div>
												<div class="content-reply">
													<font-awesome-icon icon="fa-solid fa-quote-right" style="font-size: 1rem; color: gray;padding-bottom: 5px;" />
													<div style="display: inline-block; padding: 5px 10px;" class="bind-chat-html" v-html="u.ParentComment.content_message"></div>
													<div v-if="u.ParentComment.file_path" class="r-cm p-0 m-0">
														<div style="max-width: 150px;">
															<a v-bind:href="basedomainURL+u.ParentComment.file_path" data-fancybox :data-caption="u.ParentComment.file_name">
																<img v-bind:src="basedomainURL + (u.ParentComment.file_path ||'/Portals/Image/noimg.jpg')" on-error="/Portals/Image/noimg.jpg" style="width: 100%; height: 100%; object-fit: contain; border-radius: 10px;" />
															</a>
														</div>
													</div>
												</div>
												<div class="name-date-reply" style="text-align: left; white-space: nowrap;">
													<span style="color: #888;font-size: 12px;">
														{{u.ParentComment.user_chat_name + ', ' + 
															(moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY") == moment(new Date()).format("DD/MM/YYYY") 
															? ("Hôm nay lúc " + moment(new Date(u.ParentComment.created_date)).format("HH:mm"))
															: moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY"))
														}}
													</span>
												</div>
											</div>
										</div>
										<a :href="u.content_message" target="_blank" v-if="u.content_message.includes('http://') || u.content_message.includes('https://')">
											<div v-html="u.content_message" style="display: inline-grid; text-align: -webkit-left; padding: 5px 10px; "></div>
										</a>
										<div v-else v-html="u.content_message" style="display: inline-grid; text-align: -webkit-left; padding: 5px 10px; "></div>
										<div>
											<span class="r-cdate fw-400">
												{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
											</span>
										</div>
									</div>

									<div class="r-file" v-if="u.type_message != 0 && u.type_message != 1 && u.files != null && u.files.length>0" style="padding:0">
										<ul>
											<li v-for="(f, indexFile) in u.files" :key="indexFile" class="border-none px-0 py-2">
												<div class="r-fbox image_file_chat">
													<a @click="showfile(f, u)" v-if="f.is_image!='1'">
														<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" v-if="f.is_image!='1'" />
													</a>
													<Image v-if="f.is_image=='1'"
														class="flex"
														:src="basedomainURL+f.file_path"
														style="width: 100%; height: 100%; object-fit: contain;"
														preview
													/>
													<a @click="showfile(f, u)" v-if="f.is_image!='1'" style="color:inherit">
														<b>{{f.file_name}}</b>
													</a>
													<a class="viewimg" v-if="f.is_image=='1' || f.file_type=='pdf'" 
														v-bind:href="basedomainURL+f.file_path" 
														data-fancybox :data-caption="f.file_name" 
														style="color:inherit">
													</a>
													<span v-if="f.is_image!='1'">{{formatByte(f.file_size)}}</span>
												</div>
											</li>
											<div class="pt-0 pb-3">
												<span class="r-cdate fw-400">
													{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
												</span>
											</div>
										</ul>
									</div>
									<div class="r-action">
										<div class="position-absolute flex" style="bottom: -13px; min-width: 100%;">
											<div @click="getStick(u)" style="cursor:pointer">
												<ul class="lis-n p-0 mr-2" v-if="u.sticks && u.sticks.length > 0" style="box-shadow: none;">
													<li class="flex" v-for="(st, idxSt) in u.sticks" :key="idxSt" style="border-radius: 50%;">
														<img v-bind:src="basedomainURL+st.stick_file" style="width: 25px;" />
													</li>
													<li class="flex" v-if="u.countstick - 3 > 0" style="border-radius: 50%;">
														<span class="flex pr-1" style="align-items: center;"> +{{u.countstick - 3}}</span>
													</li>
												</ul>
											</div>
											<div class="r-action hoverMes" style="cursor:pointer">
												<ul v-if="!u.IsEdit" class="lis-n p-0" style="background-color: transparent; box-shadow: none;">
													<li>
														<a v-if="!u.IsMe" class="mr-1"
															@click="showEmote($event, u)">
															<span class="badge-2 fw-400 m-0" style="font-size: 12px; " v-tooltip.top="'Tương tác'">
																<i class="pi pi-thumbs-up c-blue-700" style="font-size: 1.25rem;"></i>
															</span>
														</a>
													</li>
													<li>
														<a @click="u.IsReply?HuyReply():Reply(u)" style="margin-right: 5px;">
															<span class="badge-2 fw-400 m-0" style="font-size: 12px; " v-tooltip.top="u.IsReply?'Hủy':'Trả lời '">
																<i v-if="u.IsReply" class="pi pi-times" style="font-size: 1.25rem; color:red;"></i> 
																<font-awesome-icon v-else icon="fa-solid fa-quote-right" style="font-size: 1.25rem; color: orange;" />
																<span style="vertical-align: super;" v-if="u.CountReply != null && u.CountReply.length > 0">
																	{{((u.CountReply.length > 0) ? '('+u.CountReply.length+')' : '') }}
																</span>
															</span>
														</a>
													</li>
													<li>
														<a @click="ShareMsg(u)">
															<span class="badge-2 fw-400 m-0" style="font-size: 12px; " v-tooltip.top="'Chia sẻ'">
																<font-awesome-icon icon="fa-solid fa-share" style="font-size: 1.25rem; color: #1976d2;" /> 
															</span>
														</a>
													</li>
												</ul>
											</div>
										</div>
									</div>
									<div style="position: absolute;top: 0;right: 0;">
										<div :id="'menu' + u.chat_message_id" class="btn-group" role="group">
											<Button
												type="button"
												class="btn cur-p no-after btn-mes-func"
												icon="pi pi-ellipsis-h"
												@click="toogleFuncMes($event, u, index)"
												aria-haspopup="true"
												aria-controls="overlay_panelFuncMes"
											/>
										</div>
									</div>
								</div>
							</div>
							<div v-if="u.seens && u.seens.length > 0 && (u.IsMe || props.detailChat.is_group_chat)" class="pB-5" style="display: flex;flex-direction: column;padding-right: 8px;align-items: end;">
								<span class="format_center" style="font-size: 12px;">Đã xem</span>
								<div class="card-users m-0">
									<ul class="format_center" style="margin:0.25rem 0 0;" @click="showModalUserSeen(u)">
										<li class="flex" v-for="(sn, idx) in u.seens" :key="idx">
											<Avatar v-if="idx < 5"
												:image="sn.avatar
													? basedomainURL + sn.avatar
													: basedomainURL + '/Portals/Image/noimg.jpg'
												"
												v-bind:label="sn.avatar ? '' : (sn.last_name ?? '').substring(0, 1)"
												v-tooltip.top="sn.full_name"
												shape="circle"
												style="border: 1px solid #ccc;width: 20px;height: 20px; cursor: pointer;"
												:style="{ background: bgColor[idx % 7] + '!important'}"
											/>
										</li>
										<li v-if="u.seens.length - 5 > 0">
											<div style=" background-color: #f8fafb; color: #aaa; text-align: center; border-radius: 50%; margin-left: 0; font-size: 10px; max-height: min-content; width: 20px;height: 20px;border: solid 1px rgba(0,0,0,0.1);display: flex;align-items: center;justify-content: center;">
												<span>+{{u.seens.length-2}}</span>
											</div>
										</li>
									</ul>
								</div>
							</div>
						</div>
						<div id="nodata" v-if="props.listMessage.length == 0">
							<div style="display: table; height: calc(100vh - 260px); width: 100%">
								<div style="text-align:center;display: table-cell; vertical-align: middle;margin: auto;width: 100%;">
									<img width="200" v-bind:src="basedomainURL + '/Portals/Image/29103-chat-bubbles-2.gif'" />
									<p style="color: #aaa; margin-top: 20px; font-size: 16px"><b>Hiện chưa có tin nhắn nào</b></p>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
	<div v-if="!IsCall" class="box-footer" style=" border-radius: 10px;" :style="IsReply ? 'z-index:100;' : ''">
		<div>
			<div v-if="IsReply" style="">
				<div class="reply-chat show-reply" style="padding: 10px;background-color: antiquewhite;border-radius: 10px;margin: 10px;">
					<div class="row">
						<div class="col-12 md:col-12 flex">
							<div class="col-11 content-reply">
								<font-awesome-icon icon="fa-solid fa-quote-right" style="font-size: 1rem; color: gray;" />
								<div style="display: inline-block" class="bind-chat-html ml-2" v-html="tinnhanreply.content_message"></div>
							</div>
							<div class="col-1 close-reply text-right" v-if="isEdit != true">
								<a @click="HuyReply()"><i class="pi pi-times"></i></a>
							</div>
						</div>
						<div class="col-12 md:col-12 flex name-date-reply">
							<span style="padding-left: 10px;color: #888;font-size: 12px;">
								{{tinnhanreply.user_chat_firstname + ', ' + 
									(moment(new Date(tinnhanreply.created_date)).format("DD/MM/YYYY") == moment(new Date()).format("DD/MM/YYYY") 
									? ("Hôm nay lúc " + moment(new Date(tinnhanreply.created_date)).format("HH:mm"))
									: moment(new Date(tinnhanreply.created_date)).format("DD/MM/YYYY"))
								}}
							</span>
						</div>
					</div>

				</div>
			</div>
			<div class="r-file" v-if="FileAttach.length>0">
				<h3 style="font-weight: bold;font-size: 16px;margin: 10px 0;color: #2196f3;">Danh sách file đã chọn</h3>
				<ul class="my-2">
					<li v-for="(f, idx) in FileAttach" :key="idx">
						<div class="r-fbox">
							<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" />
							<b>{{f.file_name}}</b>
							<span>{{formatByte(f.file_size)}}</span>
							<div style="width:40px;position:absolute;top:-10px;right:-20px;">
								<a @click="removeFilesComment(FileAttach,idx)"><i style="font-size:20px" class="pi pi-times-circle"></i></a>
							</div>
						</div>
					</li>
				</ul>
			</div>
			<div class="msb-reply flex" style="background-color: #fff; border-top: solid 2px #9acffa;">
				<ul class="list-group autoUser autoUsercm" v-if="props.detailChat.Focuscm" tabindex="0" @keydown="handleKeyDowncm($event)">
					<li class="list-group-item" style="padding:5px" 
						v-for="(u, idx) in filterUsercm" :key="idx"
						@click="clickAddUsercm(u)" 
						@mouseover="$parent.focusedIndex = $index">
						<div style="display:flex">
							<img class="ava" width="40" height="40" 
								v-bind:src="u.avatar
											? basedomainURL + u.avatar
											: basedomainURL + '/Portals/Image/noimg.jpg'
										"
								v-if="u.avatar"
							/>
							<Avatar v-else
								class="avt-replace"
								size="large"
								shape="circle"
								v-bind:label="(u.last_name ?? '').substring(0, 1)"
								style="cursor: pointer;width: 40px; height: 40px;"
								:style="{ background: bgColor[idx%7] + '!important'}"
							/>
							<div style="margin-left:5px">
								<b style="padding:5px">{{u.full_name}}</b>
								<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
								<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.organization_name}}</span>
							</div>
						</div>
					</li>
				</ul>
				<div class="flex flex-1">
					<div style="flex: 1;">
						<Textarea class="scroll-width-thin div-comment" 
							style="height: 75px; padding: 0.5rem; overflow: auto; border: none; font-size: 15px; width: 100%; text-align: justify;" 
							id="noiDungChat" 
							v-model="noiDungChat.noiDung" 
							v-on:keypress="changeContent($event)"
							v-on:keydown.enter.exact.prevent="sendMS(0, noiDungChat)" 
							v-on:keydown.enter.shift.exact.prevent="noiDungChat.noiDung += '\n'" 
							placeholder="Nhập nội dung tin nhắn..."
						/>
					</div>
				</div>
				<div v-if="isEdit!=true" class="showiconchat flex" style="width: 15rem;align-items: center;">
					<button class="btn-icon-chat" data-toggle="dropdown" v-tooltip.top="'Sticker'" @click="showEmoji($event, 1)" style="right:150px;color:#ff9900">
						<font-awesome-icon icon="fa-solid fa-face-smile" />
					</button>
					<button class="btn-icon-chat" style="right:100px;color:#777" @click="chooseFile('imageChat')" v-tooltip.top="'Ảnh'">
						<font-awesome-icon icon="fa-solid fa-image" />
					</button>
					<input class="hidden" id="imageChat" type="file" multiple="true" accept="image/*" @change="PutFileUpload" />
					<button class="btn-icon-chat" style="right:50px;color:#777" @click="chooseFile('fileUpChat')" v-tooltip.top="'File đính kèm'">
						<font-awesome-icon icon="fa-solid fa-paperclip" />
					</button>
					<input class="hidden" id="fileUpChat" type="file" multiple="true" accept="*" @change="PutFileUpload" />
					<button class="btn-icon-chat" style="right:0;" v-tooltip.top="'Gửi'" :disabled="(noiDungChat==null || noiDungChat.noiDung=='') && FileAttach.length == 0" @click="sendMS(0)">
						<font-awesome-icon icon="fa-solid fa-paper-plane" />
					</button>
				</div>
				<div v-if="isEdit==true" class="showiconchat flex" style="width: 10rem;align-items: center;">
					<button class="btn-icon-chat" data-toggle="dropdown" v-tooltip.top="'Sticker'" @click="showEmoji($event, 1)" style="right:100px;color:#ff9900">
						<font-awesome-icon icon="fa-solid fa-face-smile" />
					</button>
					<button class="btn-icon-chat" style="right:50px;" v-tooltip.top="'Gửi'" :disabled="(noiDungChat==null || noiDungChat.noiDung=='') && FileAttach.length == 0" @click="sendMS(0, noiDungChat)">
						<font-awesome-icon icon="fa-solid fa-paper-plane" />
					</button>
					<button class="btn-icon-chat" style="right:0;" v-tooltip.top="'Hủy'" @click="cancelEditMessage(noiDungChat)">
						<i class="pi pi-times-circle" style="font-size: 1.75rem; color:red; font-weight: bold;"></i> 
					</button>
				</div>
			</div>
		</div>
	</div>
	<!-- panel list icon -->
	<OverlayPanel
		class="p-0 panel_emote_chat"
		ref="panelEmoij4"
		appendTo="body"
		:showCloseIcon="false"
		id="overlay_panelEmoij4"
	>
		<VuemojiPicker @emojiClick="handleEmojiClick" dataSource="/data_local_vuemoji_picker.json" />
	</OverlayPanel>
	<!-- panel menu message -->
	<OverlayPanel
		ref="funcMesChat"
		appendTo="body"
		class="p-0 m-0 panelFuncMes"
		:showCloseIcon="false"
		id="overlay_panelFuncMes"
		style="width: fit-content;"
	>
		<div>
			<ul class="ul-func-mes m-0" style="width: 10rem;">
				<li class="px-2 py-2" v-if="mesFuncClick.IsMe && mesFuncClick.type_message == 0">
					<a @click="EditMessage(mesFuncClick)" class="d-b td-n">
						<!-- <i class="pi pi-pencil"></i>  -->
						<font-awesome-icon icon="fa-solid fa-pencil" />
						<span class="ml-1"> Chỉnh sửa</span>
					</a>
				</li>
				<li class="px-2 py-2"> <a @click="Reply(mesFuncClick)" class="d-b td-n">
					<!-- <i class="las la-quote-right "></i>  -->
					<font-awesome-icon icon="fa-solid fa-quote-right" />
					<span class="ml-1"> Trả lời</span></a>
				</li>
				<li class="px-2 py-2"> <a @click="ShareMsg(mesFuncClick, 'overlay')" class="d-b td-n">
					<!-- <i class="las la-share"></i>  -->
					<font-awesome-icon icon="fa-solid fa-share" /> 
					<span class="ml-1"> Chia sẻ</span></a>
				</li>
				<li class="px-2 py-2" v-if="mesFuncClick.type_message == 1 || mesFuncClick.type_message == 2 || mesFuncClick.type_message == 3 || mesFuncClick.type_message == 4"> 
					<a @click="openFile(mesFuncClick)" class="d-b td-n">
					<i class="pi pi-download"></i>
					<span class="ml-1"> Tải xuống</span></a>
				</li>
				<li v-if="mesFuncClick.IsMe" role="separator" class="divider"></li>
				<li class="px-2 py-2" v-if="props.detailChat.isdelcomment || mesFuncClick.IsMe"> 
					<a @click="Del_Message(mesFuncClick,mesFuncClick.index)" class="d-b td-n" style="color:red;">
						<i class="pi pi-trash"></i> 
						<!-- <font-awesome-icon icon="fa-solid fa-trash-o" />  -->
						<span class="ml-1"> Xóa</span>
					</a>
				</li>
			</ul>
		</div>
	</OverlayPanel>
	<!-- panel list emote -->
	<OverlayPanel
		class="p-0 p-overlaypanel-setup"
		ref="panelEmote"
		appendTo="body"
		:showCloseIcon="false"
		id="overlay_panelEmote"
		>
		<div class="flex p-0">
			<div class="p-0 cursor-pointer format-center"
				style="width: 40px; height: 40px"					
				@mouseover="onCheckHover(item)"
				@mouseleave="hideCheckHover()"
				v-for="(item, index) in emoteList" :key="index"
			>
				<img v-tooltip.top="item.emote_name"
					:src="basedomainURL + item.emote_file"
					:alt="item.emote_file"
					@click="addEmote(item)"
					:class="checkHover == item.emote_id
						? 'animate__animated  animate__pulse emote-lg'
						: 'emote-md'
					"
				/>
			</div>
		</div>
	</OverlayPanel>
	<!-- panel button download file -->
	<OverlayPanel
		ref="funcMesFiles"
		appendTo="body"
		class="p-0 m-0 panelFuncFiles"
		:showCloseIcon="false"
		id="overlay_panelFuncFiles"
		style="width: fit-content;"
	>
		<div>
			<ul class="ul-func-mes m-0" style="width: 10rem; padding:0; list-style-type: none;">
				<li class="px-2 py-2">
					<a @click="openFile(fileFuncClick,fileFuncClick.file_path)" class="d-b td-n">
						<i class="pi pi-download"></i>
						<span class="ml-1"> Tải xuống</span>
					</a>
				</li>
			</ul>
		</div>
	</OverlayPanel>
	<!-- Dialog share message -->
	<Dialog
		:header="headerShareChat"
		v-model:visible="displayShareChat"
		:autoZIndex="true"
		:modal="true"
		style="z-index: 1000;width:50vw;"
	>
		<form>
			<div class="grid formgrid m-2">
				<div class="col-12 md:col-12 flex p-0">
					<div class="col-6">
						<div class="flex" style="flex-direction:column;">
							<div style=" border: solid 1px #ced4da; margin-bottom: 5px; border-radius: 5px;">
								<input class="ipautoUser" 
									v-model="share.searchU" 
									style="width: 100%; border: none; background-color: transparent; padding: 7px 5px;" 
									placeholder="Tìm kiếm hội thoại cần chia sẻ" />
							</div>
							<div class="" tabindex="-1" style="overflow-x: hidden; overflow-y: auto; height: calc(100vh - 40rem); width: -webkit-fill-available;">
								<div class="scrollParenttree" style="overflow-x: hidden; overflow-y: auto;">
									<ul class="list-group dataUser p-0 m-0" tabindex="0">
										<div class="p-0">
											<div class="p-3" style="background-color: antiquewhite;">
												<span class="font-bold uppercase" style="color: #333;">Trò chuyện gần đây</span>
											</div>											
											<li class="list-group-item format_center"
												style="padding:0.5rem 0.25rem;" 
												v-for="(u, index) in filterListChatRecent()" :key="index">
												<div class="" style="display: -webkit-box; display: -webkit-inline-box; justify-content: flex-start; width: 100%;display: flex; align-items: center;">
													<Checkbox class="mr-3" :binary="u.checkShare" v-model="u.checkShare" @change="clickAddShareMsg(u, 1)" />
													<img class="ava" width="48" height="48" 
														v-bind:src="u.avatar_group
																	? basedomainURL + u.avatar_group
																	: basedomainURL + '/Portals/Image/image_group_user.jpg'
																"
														v-if="u.avatar_group"
													/>
													<Avatar v-else
														class="avt-replace"
														shape="circle"
														v-bind:label="(u.chat_group_lastname ?? '').substring(0, 1)"
														style="cursor: pointer;width: 48px; height: 48px; border: 1px solid #e7e7e7;"
														:style="{ background: bgColor[index%7] + '!important'}"
													/>
													<div class="format_text" style="margin-left:5px;">
														<b style="padding:5px">{{u.chat_group_name}}</b>
													</div>
												</div>
											</li>
											<div class="p-3" style="background-color: antiquewhite;">
												<span class="font-bold uppercase" style="color: #333;">Danh sách người dùng</span>
											</div>											
											<li class="list-group-item format_center"
												style="padding:0.5rem 0.25rem;" 
												v-for="(u, index) in filterListUserAll()" :key="index">
												<div class="" style="display: -webkit-box; display: -webkit-inline-box; justify-content: flex-start; width: 100%;display: flex; align-items: center;">
													<Checkbox class="mr-3" :binary="u.checkShare" v-model="u.checkShare" @change="clickAddShareMsg(u, 2)" />
													<img class="ava" width="48" height="48" 
														v-bind:src="u.avatar
																	? basedomainURL + u.avatar
																	: basedomainURL + '/Portals/Image/noimg.jpg'
																"
														v-if="u.avatar"
													/>
													<Avatar v-else
														class="avt-replace"
														shape="circle"
														v-bind:label="(u.last_name ?? '').substring(0, 1)"
														style="cursor: pointer;width: 48px; height: 48px;"
														:style="{ background: bgColor[u.is_order % 7] + '!important', }"
													/>
													<div class="format_text" style="margin-left:5px;">
														<b style="padding:5px">{{u.full_name}}</b>
													</div>
												</div>
											</li>
										</div>
									</ul>
								</div>
							</div>
						</div>
					</div>
					<div class="col-6">
						<div class="flex">
							<label class="field col-12 p-0 font-bold text-left flex" style="align-items:center;">
								Đã chọn
							</label>
						</div>
						<div class="flex">
							<div class="ip-us" tabindex="-1" style="overflow-x: hidden; overflow-y: auto; height: calc(100vh - 40rem); width: -webkit-fill-available;">
								<ul class="my-0" style="float: left; padding: 0; width: 100%;">
									<li style="width: 100%; height: 36px; margin: 5px 0; float: left; list-style-type: none " 
										v-for="(m, index) in listChatShare" :key="index"
									>
										<div class="label-chat btn-info-user-chat" style="width: 100%; white-space: unset; display: inline-flex; justify-content: space-between;">
											<div class="flex" style="width: 100%;">
												<img class="ava" width="32" height="32" 
													v-bind:src="m.avatar
																? basedomainURL + m.avatar
																: basedomainURL + '/Portals/Image/noimg.jpg'
															"
													:style="m.user_join == chat.created_by ? 'border:solid 2px orange;': ''"
													v-if="m.avatar"
												/>
												<Avatar v-else
													class="avt-replace"
													shape="circle"
													v-bind:label="(m.last_name ?? '').substring(0, 1)"
													style="cursor: pointer;width: 32px; height: 32px;"
													:style="{ background: bgColor[index % 7] + '!important', }"
												/>
												<strong style="padding: 7px; font-size: 14px; font-weight: 500;">{{m.full_name}}</strong>
											</div>
										</div>
									</li>
								</ul>
							</div>
						</div>
					</div>
				</div>
				<div class="col-12 md:col-12 flex p-0" style="border-top: solid 1px rgba(0,0,0,0.1); min-height: 18rem;">
					<div class="col-12 py-3">
						<label class="font-bold" for="inputState">Nội dung chia sẻ</label>
						<div v-if="(msgShare.type_message === 1 && msgShare.FileAttach_Share.length > 0) || (msgShare.type_message === 2 && msgShare.FileAttach_Share.length > 0)" 	
							class="r-file pX-0">
							<ul class="p-0 mb-0">
								<li v-if="msgShare.type_message === 1 && msgShare.FileAttach_Share.length > 0" class="m-0 p-0">
									<div class="r-cm p-0 m-0">
										<div style="max-width: 300px;">
											<Image class="flex img-share"
												:src="basedomainURL + (msgShare.FileAttach_Share[0].file_path ||'/Portals/Image/noimg.jpg')"
												preview
											/>
										</div>
									</div>
								</li>
								<li v-else class="m-0" v-for="(f, idx) in msgShare.FileAttach_Share" :key="idx">
									<div class="r-fbox" v-if="msgShare.type_message === 2">
										<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" />
										<b>{{f.file_name}}</b>
										<span>{{formatByte(f.file_size)}}</span>
									</div>
								</li>
							</ul>
						</div>
						<div class="msb-reply mt-2 flex">
							<Textarea class="scroll-width-thin div-comment" 
								style="resize: none; min-height: 75px; width: 100%; padding: 10px; overflow: auto; background-color: transparent; 
									border: solid 1px #bfbfbf; font-size: 15px; color: #000;
									border-radius: 10px;
									/*border-right: none; border-top-left-radius: 10px; border-bottom-left-radius: 10px;*/
								" 
								id="noiDungChat_Share" 
								autoResize
								v-model="msgShare.content_message" 
								placeholder="Nhập nội dung tin nhắn..." disabled>
							</Textarea>
							<!-- <div class="showiconchat">
								<button class="btn-icon-chat px-3" data-toggle="dropdown" v-tooltip.top="'Sticker'" @click="showEmoji($event, 1)" 
									style="right:0;color:#ff9900; border: 1px solid #d9d9d9; border-left: none; border-top-right-radius: 10px; border-bottom-right-radius: 10px;">
									<font-awesome-icon icon="fa-solid fa-face-smile" />
								</button>
							</div> -->
						</div>
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialog()"
				class="p-button-text"
			/>

			<Button label="Chia sẻ" icon="pi pi-send" @click="shareMesChat()" />
		</template>
	</Dialog>
	<!-- Dialog show user using stick for message -->
	<Dialog v-model:visible="displayStick"
		:dismissableMask="true"
		:modal="true"
		style="z-index:1000;"
		:header="'Tương tác'"
		class="dialog-stick"
	>
	<div style="background-color:#fff;padding:0 10px 10px;border-top: 1px solid #f5f5f5;">
		<TabView lazy :active-index="tabStickActive" @tab-change="changeTabStick">
			<TabPanel>
				<template #header>
					<span>Tất cả ({{emote_message.length}})</span>
				</template>
				<div class="sti-row">
					<div class="row-comment" v-for="(u, idx) in emote_message" :key="idx">
						<div style="display:flex; width:100%;">
							<div style="position: relative;" v-if="u.avatar">
								<img class="ava" width="48" height="48" 
									v-bind:src="u.avatar
												? basedomainURL + u.avatar
												: basedomainURL + '/Portals/Image/noimg.jpg'
											"
								/>
								<span style="position: absolute;width: 1rem;height: 1rem;bottom: 3px;left: 35px;">
									<img v-bind:src='basedomainURL + u.stick_file' style="height: 16px;" />
								</span>
							</div>
							<Avatar v-else
								class="avt-replace"
								size="large"
								shape="circle"
								v-bind:label="(u.last_name ?? '').substring(0, 1)"
								style="cursor: pointer;width: 48px; height: 48px;"
								:style="{ background: bgColor[idx%7] + '!important'}"
							/>
							<div style="margin-left:5px;width: -webkit-fill-available;">
								<div class="flex align-items-center" style="padding:5px">
									<b class="flex-1">{{u.full_name}}</b>
									<span class="r-cdate">{{moment(new Date(u.created_date)).format("HH:mm DD/MM/YYYY")}}</span>
								</div>
								<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
								<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.department_name}}</span>
							</div>
						</div>
					</div>
				</div>
			</TabPanel>
			<TabPanel v-for="(st, index) in emoteList" :key="index">
				<template #header>
					<img v-bind:src="basedomainURL + st.emote_file" 
						style="width: 32px !important; height: 32px !important; border-radius: 50%;" 
					/> 
					<span class="pl-1" v-if="st.sticks.length>0">({{st.sticks.length}})</span>
				</template>
				<div class="sti-row">
					<div class="row-comment" v-for="(u, idx) in st.sticks" :key="idx">
						<div style="display:flex; width:100%;">
							<img class="ava" width="48" height="48" 
								v-bind:src="u.avatar
											? basedomainURL + u.avatar
											: basedomainURL + '/Portals/Image/noimg.jpg'
										"
								v-if="u.avatar"
							/>
							<Avatar v-else
								class="avt-replace"
								size="large"
								shape="circle"
								v-bind:label="(u.last_name ?? '').substring(0, 1)"
								style="cursor: pointer;width: 48px; height: 48px;"
								:style="{ background: bgColor[idx%7] + '!important'}"
							/>
							<div style="margin-left:5px;width: -webkit-fill-available;">
								<div class="flex align-items-center" style="padding:5px;">
									<b class="flex-1">{{u.full_name}}</b>
									<span class="r-cdate">{{moment(new Date(u.created_date)).format("HH:mm DD/MM/YYYY")}}</span>
								</div>
								<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
								<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.department_name}}</span>
							</div>
						</div>
					</div>
				</div>
			</TabPanel>
		</TabView>
	</div>
	</Dialog>
	<!-- Dialog list user seen -->
	<Dialog v-model:visible="displaySeen"
		:dismissableMask="true"
		:modal="true"
		:header="'Danh sách người dùng đã xem'"
		class="dialog-stick"
		style="width:35rem;z-index:1000;"
	>
		<div style="background-color:#fff;padding:0 0 10px;border-top: 1px solid #f5f5f5;">
			<div class="sti-row">
				<div class="row-comment" v-for="(u, idx) in listUserSeen" :key="idx" style="border-top:1px solid #f3f3f3;">
					<div style="display:flex; width:100%; align-items: center;">
						<div style="position: relative;" v-if="u.avatar">
							<img class="ava" width="48" height="48" 
								v-bind:src="u.avatar
											? basedomainURL + u.avatar
											: basedomainURL + '/Portals/Image/noimg.jpg'
										"
							/>
						</div>
						<Avatar v-else
							class="avt-replace"
							size="large"
							shape="circle"
							v-bind:label="(u.last_name ?? '').substring(0, 1)"
							style="cursor: pointer;width: 48px; height: 48px;"
							:style="{ background: bgColor[idx%7] + '!important'}"
						/>
						<div style="margin-left:5px;width: -webkit-fill-available;">
							<div class="flex align-items-center" style="padding:5px">
								<b class="flex-1">{{u.full_name}}</b>
								<span class="r-cdate">{{u.last_seen_date != null ? moment(new Date(u.last_seen_date)).format("HH:mm DD/MM/YYYY") : ''}}</span>
							</div>
							<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
							<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.department_name}}</span>
						</div>
					</div>
				</div>
			</div>
		</div>
	</Dialog>
</template>
<style scoped>
	@import url(./stylechat.css);
</style>
<style lang="scss" scoped>
	::v-deep(.image_preview_chat) {
		.p-image-preview-container img {
			width: 100%;
			height: 100%;
			object-fit: contain;
			border-radius: 20px 20px 0px 0px;
		}
		.p-image-preview-container:hover > .p-image-preview-indicator {
			border-radius: 20px 20px 0px 0px;
		}
	}
	::v-deep(.image_file_chat) {
		.p-image-preview-container img {
			width: 100%;
			height: 100%;
			object-fit: contain;
		}
	}
	::v-deep(.img-share) {
		img {
			width: 100%;
			height: 100%;
			object-fit: contain;
		}
	}
	::v-deep(.avt-replace) {
		.p-avatar-text {
			display: flex;
			width: inherit;
			align-items: center;
			justify-content: center;
			font-size: initial;
			text-transform: uppercase;
			color: #fff;
		}
	}
</style>

