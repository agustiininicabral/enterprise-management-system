import type {TaskStatus} from './api';
export function statusLabel(status:TaskStatus):string{return status==='InProgress'?'In progress':status}
