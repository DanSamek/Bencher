## Worker

### Build

#### Requirements
- `Linux`
- `.net 9.0+`
- `GCC >= 7.3.0 or Clang >= 8.0.0`
- `git`

#### How to run
- `cd Worker`
- `dotnet run -c Release`

### Run

- `Web application url` = URL of the webapp - for example `https://bencher-testing.xyz`
- `Access token` = Generated access token in the webapp
- `Number of threads to use` = how many threads are used for game run using `fastchess`
- `Try split threads` = experimental implementation of thread split if there are multiple paused tests. *NOTE: This option is only enabled if worker has atleast 16 threads that are used.*