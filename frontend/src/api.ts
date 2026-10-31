export type TaskStatus = 'Todo'|'InProgress'|'Done';
export interface Task {id:string;title:string;description:string|null;status:TaskStatus;createdAt:string;updatedAt:string}
export interface AuthResponse {accessToken:string;expiresAt:string;email:string}
const baseUrl=import.meta.env.VITE_API_BASE_URL??'';
async function request<T>(path:string,token?:string,init:RequestInit={}):Promise<T>{
 const response=await fetch(`${baseUrl}/api${path}`,{...init,headers:{'Content-Type':'application/json',...(token?{Authorization:`Bearer ${token}`}:{}),...init.headers}});
 if(!response.ok){const body:unknown=await response.json().catch(()=>null);const message=typeof body==='object'&&body!==null&&'error'in body?String(body.error):`Request failed (${response.status})`;throw new Error(message)}
 if(response.status===204)return undefined as T;
 return response.json() as Promise<T>;
}
export const api={
 register:(email:string,password:string)=>request<AuthResponse>('/auth/register',undefined,{method:'POST',body:JSON.stringify({email,password})}),
 login:(email:string,password:string)=>request<AuthResponse>('/auth/login',undefined,{method:'POST',body:JSON.stringify({email,password})}),
 tasks:(token:string)=>request<Task[]>('/tasks',token),
 createTask:(token:string,title:string,description:string)=>request<Task>('/tasks',token,{method:'POST',body:JSON.stringify({title,description})}),
 updateTask:(token:string,task:Task,status:TaskStatus)=>request<Task>(`/tasks/${task.id}`,token,{method:'PUT',body:JSON.stringify({title:task.title,description:task.description,status})}),
 deleteTask:(token:string,id:string)=>request<void>(`/tasks/${id}`,token,{method:'DELETE'})
};
