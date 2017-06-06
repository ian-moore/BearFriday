module BearFriday.Instagram

let buildAuthUrl =
    sprintf "https://api.instagram.com/oauth/authorize/?client_id=%s&redirect_uri=%s&response_type=code"