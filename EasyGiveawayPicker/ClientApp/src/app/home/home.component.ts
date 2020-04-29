import { Component, Inject, ViewEncapsulation, AfterViewInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Tweet } from '../models/tweet';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent {
  public tw: Tweet = new Tweet();
  public isLoading: boolean = false;
  public winnerSelected: boolean = false;
  public giveawayTweet: string;
  constructor(public http: HttpClient) {
  }

  public getTweets() {
    this.winnerSelected = true;
    this.isLoading = true;
    this.http.get<Tweet>('/tweet/SelectWinner', { params: new HttpParams().set("giveawayLink", this.giveawayTweet) }).subscribe(result => {
      this.tw = result;
      (<any>window).twttr.widgets.load();
      this.isLoading = false;
    });
  }

}
