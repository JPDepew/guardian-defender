import datetime
from google.cloud import datastore
datastore_client = datastore.Client(namespace='scores')
SCORE_KIND = 'user_score'

def post_score(request):
    """ Create a new user score """
    print(request)
    print(request.get_json())
    if not 'name' in request.form or not 'score' in request.form:
        return 'Name or score not provided', 400

    create_score(request.form['name'], request.form['score'])

    return 'Success', 200

def create_score(name, score):
    entity = datastore.Entity(key=datastore_client.key(SCORE_KIND))
    entity.update({
        'name': name,
        'score': int(score),
        'create_date': datetime.datetime.now(),
    })

    datastore_client.put(entity)
