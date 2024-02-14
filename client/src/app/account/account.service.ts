import { Injectable } from '@angular/core';
import { BehaviorSubject, ReplaySubject, map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Address, User } from '../shared/models/user';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
baseUrl = environment.apiUrl;
private currentuserSource = new ReplaySubject<User | null>(1);
currentUser$ = this.currentuserSource.asObservable();

  constructor(private http:HttpClient, private router : Router) { }

  loadCurrentUser(token: string | null)
  {
    if(token===null){
      this.currentuserSource.next(null);
      return of(null);
    }

    let headers = new HttpHeaders();
    headers = headers.set('Authorization',`Bearer ${ token}`);
    return this.http.get<User>(this.baseUrl+ 'account',{headers}).pipe(
      map(user => {
        if(user){
          localStorage.setItem('token',user.token);
          this.currentuserSource.next(user);
          return user;
        }
        else{
          return null;
        }
        
      })
    )
  }

  login(values: any){
    return this.http.post<User>(this.baseUrl +'account/login',values).pipe(
      map(user => {
        localStorage.setItem('token',user.token);
        this.currentuserSource.next(user);
      })
    )
  }

  register(values: any){
    return this.http.post<User>(this.baseUrl + 'account/register',values).pipe(
      map(user => {
        localStorage.setItem('token',user.token);
        this.currentuserSource.next(user);
      })
    )
  }

  logout(){
    localStorage.removeItem('token');
    this.currentuserSource.next(null);
    this.router.navigateByUrl('/');

  }

  checkEmailExists(email: string)
  {
    return this.http.get<boolean>(this.baseUrl + 'account/emailExists?email='+ email);
  }

  getUserAddress(){
    return this.http.get<Address>(this.baseUrl + 'account/address');
  }

  updateUserAddress(address : Address){
    return this.http.put(this.baseUrl + 'account/address', address);
  }

}
