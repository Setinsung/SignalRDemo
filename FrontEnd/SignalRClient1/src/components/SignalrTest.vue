<script setup lang="ts">
import { reactive, ref } from 'vue';
import axios from 'axios';
import * as signalR from '@microsoft/signalr';
interface LoginResp {
  name: string;
  token: string;
}
const myName = ref("");
const testResp = ref("");
const loginInfo = reactive({
  userName: "wtf",
  password: "123456"
});

const userMessage = ref("");
const privateMessage = reactive({
  toUserName: "",
  message: ""
});


const messages = ref<string[]>([]);

let signalRConn: signalR.HubConnection;

const startsignalRConn = async (token: string | undefined) => {
  if (signalRConn) await signalRConn.stop();
  signalRConn = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5021/Hubs/ChatRoomHub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => token ?? localStorage.getItem("token") ?? ""
    })
    .withAutomaticReconnect()
    .build();
  try {
    await signalRConn.start();
  } catch (err) {
    alert("SignalR连接失败，" + err);
    return;
  }
  signalRConn.on("ReceivePublicMessage", (msg: string) => { // 监听服务端发送的消息
    messages.value.push(msg);
  });
  signalRConn.on("ReceicePrivateMessage", (currentUser: string, toUser: string, time: string, msg: string) => {
    messages.value.push(`${currentUser}在${time}对${toUser}私信: ${msg}`);
  });
};

const Login = async () => {
  try {
    const res = await axios.post<LoginResp>('http://localhost:5021/api/Test/Login', loginInfo);
    alert("登录成功");
    localStorage.setItem("token", res.data.token);
    startsignalRConn(res.data.token);
    myName.value = res.data.name;
  } catch (error) {
    loginInfo.password = "";
    loginInfo.userName = "";
    alert("登录失败，" + error);
  }
};

const LoginOut = async () => {
  if (signalRConn) await signalRConn.stop(); // 停止连接
  localStorage.removeItem("token");
};

const testToken = async () => {
  const res = await axios.get<string>('http://localhost:5021/api/Hello', {
    headers: {
      Authorization: `Bearer ${localStorage.getItem("token") ?? ""}`
    }
  });
  testResp.value = res.data;
};

const sendMsg = async () => {
  try {
    await signalRConn.invoke("SendPublicMessage", userMessage.value); // 客户端向服务器端发送消息
    userMessage.value = "";
  } catch (error) {
    alert("无法发送消息，" + error);
  }
};
const sendPrivateMsg = async () => {
  try {
    await signalRConn.invoke("SendPrivateMessage", privateMessage.toUserName, privateMessage.message);
    privateMessage.message = "";
    privateMessage.toUserName = "";
  } catch (error) {
    alert("无法发送消息，" + error);
  }
};
</script>

<template>
  <fieldset>
    <legend>
      SignalRClient1
      <span v-show="myName"> - 当前用户: {{ myName }}</span>
    </legend>
    <div>
      用户名：<input type="text" v-model="loginInfo.userName">
    </div>
    <div>
      密&nbsp;&nbsp;&nbsp;码：<input type="text" v-model="loginInfo.password">
    </div>
    <hr>
    <button @click="Login">登录</button>
    <button @click="LoginOut">登出</button>
    <hr>
    <button @click="testToken">测试登录后请求</button>
    <div>{{ testResp }}</div>
    <hr>
    广播消息：<input type="text" v-model="userMessage" @keyup.enter="sendMsg" />
    <legend>消息屏</legend>
    <div>
      <ul>
        <li v-for="(msg, index) in messages" :key="index">{{ msg }}</li>
      </ul>
    </div>
    <hr>
    <legend>私聊</legend>
    <div>
      私聊对<input type="text" v-model="privateMessage.toUserName" />
      说<input type="text" v-model="privateMessage.message" @keyup.enter="sendPrivateMsg" />
    </div>
  </fieldset>
</template>