# How to deploy to freeasphosting.net

1. VS -> rebuild solution
2. ng -> `ng b --base-href=/dist/` (or run ngb.bat)
3. create different folder in your cpu for deploy (like `MEAT-MEAL-Deploy`)
4. copy/override `bin` folder from C#
5. in deploy folder, create `dist` folder
6. copy ng files there
7. cut-n-paste the `index.html` to top folder
8. if changes were made there (or initial install) copy `global.asax`, `global.asax.cs`, `web.config`. (also connect to your db with ssms and run scripts)
9. otherwise download copy of webconfig from server. this repo does not include `web.config` to protect keys.
10. ziv everything from within the deploy folder. NOT the deploy folder itself.
11. login to `https://freeasphosting.net/`, delete all, upload and unzip.
12. might need cach cleaning


## troubleshooting

`Failed to load module script: Expected a JavaScript-or-Wasm module script but the server responded with a MIME type of "text/html". Strict MIME type checking is enforced for module scripts per HTML spec.` - your index.html has links to JS files that dont exists in the DIST folder. maybe its the wrong html, maybe you missed something, check files names in html and dist.
