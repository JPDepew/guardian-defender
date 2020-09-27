import json
from google.cloud import datastore
datastore_client = datastore.Client(namespace='scores')
SCORE_KIND = 'user_score'

def get_scores(request):
    """ Return a list of names and scores """
    return json.dumps(fetch_scores(60))

def fetch_scores(limit):
    query = datastore_client.query(kind=SCORE_KIND)
    # query.order = ['-score']
    query.projection = ['score', 'name']

    scores = list(query.fetch(limit=limit))
    print(scores)

    return scores
