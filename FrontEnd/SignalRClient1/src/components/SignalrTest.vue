<script setup lang="ts">
import { ref } from 'vue';
import axios from 'axios';
import * as signalR from '@microsoft/signalr';
const userMessage = ref("");
const loginInfo = ref({
  userName: "wtf",
  password: "123456"
});
const testResp = ref("");
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
};

const Login = async () => {
  try {
    const res = await axios.post<string>('http://localhost:5021/api/Test/Login', loginInfo.value);
    alert("登录成功");
    localStorage.setItem("token", res.data);
    startsignalRConn(res.data);
  } catch (error) {
    loginInfo.value.password = "";
    loginInfo.value.userName = "";
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
</script>

<template>
  <fieldset>
    <legend>SignalRClient1</legend>
    <div>
      用户名：<input type="text" v-model="loginInfo.userName">
    </div>
    <div>
      密&nbsp;&nbsp;&nbsp;码：<input type="text" v-model="loginInfo.password">
    </div>
    <button @click="Login">登录</button>
    <button @click="LoginOut">登出</button>
    <hr>
    <button @click="testToken">测试登录后请求</button>
    <div>{{ testResp }}</div>
    <hr>
    <legend>公屏：</legend>
    <input type="text" v-model="userMessage" @keyup.enter="sendMsg" />
    <div>
      <ul>
        <li v-for="(msg, index) in messages" :key="index">{{ msg }}</li>
      </ul>
    </div>
  </fieldset>
</template>