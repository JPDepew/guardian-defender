# Copyright 2018 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import datetime
import json

from flask import Flask, render_template, request

# [START gae_python37_datastore_store_and_fetch_times]
from google.cloud import datastore

datastore_client = datastore.Client(namespace='scores')
SCORE_KIND = 'user_score'

# [END gae_python37_datastore_store_and_fetch_times]
app = Flask(__name__)


# [START gae_python37_datastore_store_and_fetch_times]
def create_score(name, score):
    entity = datastore.Entity(key=datastore_client.key(SCORE_KIND))
    entity.update({
        'name': name,
        'score': int(score),
    })

    datastore_client.put(entity)


def fetch_scores(limit):
    query = datastore_client.query(kind=SCORE_KIND)
    query.order = ['-score']

    scores = list(query.fetch(limit=limit))
    print(scores)

    return scores


@app.route('/create_user_score/', methods=['POST'])
def create_user_score():
    """ Create a new user score """
    if not 'name' in request.form or not 'score' in request.form:
        return 'Name or score not provided', 400

    create_score(request.form['name'], request.form['score'])

    return 'Success', 200


@app.route('/get_user_scores/', methods=['GET'])
def get_user_scores():
    """ Return a list of names and scores """
    return json.dumps(fetch_scores(30))


@app.route('/')
def root():
    scores = fetch_scores(10)
    return render_template(
        'index.html', scores=scores)


if __name__ == '__main__':
    # This is used when running locally only. When deploying to Google App
    # Engine, a webserver process such as Gunicorn will serve the app. This
    # can be configured by adding an `entrypoint` to app.yaml.

    # Flask's development server will automatically serve static files in
    # the "static" directory. See:
    # http://flask.pocoo.org/docs/1.0/quickstart/#static-files. Once deployed,
    # App Engine itself will serve those files as configured in app.yaml.
    app.run(host='127.0.0.1', port=8080, debug=True)
