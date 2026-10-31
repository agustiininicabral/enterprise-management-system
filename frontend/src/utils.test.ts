import {describe,expect,it} from 'vitest';
import {statusLabel} from './utils';
describe('statusLabel',()=>{
 it('formats in-progress state',()=>expect(statusLabel('InProgress')).toBe('In progress'));
 it('keeps simple states readable',()=>{expect(statusLabel('Todo')).toBe('Todo');expect(statusLabel('Done')).toBe('Done')});
});
