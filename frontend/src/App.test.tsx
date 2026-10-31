import {beforeEach,describe,expect,it,vi} from 'vitest';
import {fireEvent,render,screen} from '@testing-library/react';
import App from './App';
import {api} from './api';

describe('App authentication and task flow',()=>{
 beforeEach(()=>{
  sessionStorage.clear();
  vi.restoreAllMocks();
 });
 it('authenticates and displays the task workspace',async()=>{
  vi.spyOn(api,'login').mockResolvedValue({accessToken:'test-token',expiresAt:new Date().toISOString(),email:'qa@example.com'});
  vi.spyOn(api,'tasks').mockResolvedValue([]);
  render(<App/>);
  fireEvent.change(screen.getByLabelText('Email address'),{target:{value:'qa@example.com'}});
  fireEvent.change(screen.getByLabelText('Password'),{target:{value:'long-password'}});
  fireEvent.click(screen.getByRole('button',{name:'Sign in'}));
  expect(await screen.findByText('Your tasks')).toBeInTheDocument();
  expect(api.tasks).toHaveBeenCalledWith('test-token');
 });
 it('creates a task and renders it in the list',async()=>{
  sessionStorage.setItem('enterprise.accessToken','test-token');
  sessionStorage.setItem('enterprise.email','qa@example.com');
  vi.spyOn(api,'tasks').mockResolvedValue([]);
  vi.spyOn(api,'createTask').mockResolvedValue({
   id:'task-1',title:'Verify release',description:'Run regression suite',status:'Todo',
   createdAt:new Date().toISOString(),updatedAt:new Date().toISOString()
  });
  render(<App/>);
  fireEvent.change(await screen.findByLabelText('Task title'),{target:{value:'Verify release'}});
  fireEvent.change(screen.getByLabelText('Task description'),{target:{value:'Run regression suite'}});
  fireEvent.click(screen.getByRole('button',{name:'Add task'}));
  expect(await screen.findByText('Verify release')).toBeInTheDocument();
  expect(api.createTask).toHaveBeenCalledWith('test-token','Verify release','Run regression suite');
 });
});
