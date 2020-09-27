### Deploy cloud function

gcloud functions deploy get_scores --runtime python38 --trigger-http

### Deploy indexes (Necessary for querying)

gcloud app deploy index.yaml
