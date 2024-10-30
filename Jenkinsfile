pipeline {

    agent any

    stages {

        stage('Packaging') {
            steps {
                
                sh 'docker build --pull -f Dockerfile -t smartfarmapi:latest .'
                
            }
        }

        stage('Push to DockerHub') {
            steps {
                withDockerRegistry(credentialsId: 'dockerhub', url: 'https://index.docker.io/v1/') {
                    sh 'docker tag smartfarmapi:latest chalsfptu/smartfarmapi:latest'
                    sh 'docker push chalsfptu/smartfarmapi:latest'
                }
            }
        }

        stage('Deploy BE to DEV') {
    steps {
        withCredentials([
            string(credentialsId: 'SECRET_KEY', variable: 'SECRET_KEY'), 
            string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'), 
            string(credentialsId: 'DB_NAME', variable: 'DB_NAME'), 
            string(credentialsId: 'DB_USER', variable: 'DB_USER'), 
            string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD'), 
            string(credentialsId: 'DB_TRUST_SERVER_CERTIFICATE', variable: 'DB_TRUST_SERVER_CERTIFICATE'), 
            string(credentialsId: 'DB_MULTIPLE_ACTIVE_RESULT_SETS', variable: 'DB_MULTIPLE_ACTIVE_RESULT_SETS'), 
            string(credentialsId: 'CLIENT_ID', variable: 'CLIENT_ID'), 
            string(credentialsId: 'CLIENT_SECRET', variable: 'CLIENT_SECRET'),
            string(credentialsId: 'MailSettings__Server', variable: 'MailSettings__Server'),
            string(credentialsId: 'MailSettings__Port', variable: 'MailSettings__Port'),
            string(credentialsId: 'MailSettings__SenderName', variable: 'MailSettings__SenderName'),
            string(credentialsId: 'MailSettings__SenderEmail', variable: 'MailSettings__SenderEmail'),
            string(credentialsId: 'MailSettings__UserName', variable: 'MailSettings__UserName'),
            string(credentialsId: 'MailSettings__PassWord', variable: 'MailSettings__PassWord'),
            string(credentialsId: 'FIREBASE_API_KEY', variable: 'FIREBASE_API_KEY'),
            string(credentialsId: 'FIREBASE_AUTH_EMAIL', variable: 'FIREBASE_AUTH_EMAIL'),
            string(credentialsId: 'FIREBASE_AUTH_PASSWORD', variable: 'FIREBASE_AUTH_PASSWORD'),
            string(credentialsId: 'FIREBASE_BUCKET', variable: 'FIREBASE_BUCKET')
        ]) {
            echo 'Deploying and cleaning'
            sh 'docker container stop smartfarmapi || echo "this container does not exist"'
            sh 'echo y | docker system prune'
            sh '''docker container run \
                -e SECRET_KEY="${SECRET_KEY}" \
                -e DB_SERVER="${DB_SERVER}" \
                -e DB_NAME="${DB_NAME}" \
                -e DB_USER="${DB_USER}" \
                -e DB_PASSWORD="${DB_PASSWORD}" \
                -e DB_TRUST_SERVER_CERTIFICATE="${DB_TRUST_SERVER_CERTIFICATE}" \
                -e DB_MULTIPLE_ACTIVE_RESULT_SETS="${DB_MULTIPLE_ACTIVE_RESULT_SETS}" \
                -e CLIENT_ID="${CLIENT_ID}" \
                -e CLIENT_SECRET="${CLIENT_SECRET}" \
                -e MailSettings__Server="${MailSettings__Server}" \
                -e MailSettings__Port="${MailSettings__Port}" \
                -e MailSettings__SenderName="${MailSettings__SenderName}" \
                -e MailSettings__SenderEmail="${MailSettings__SenderEmail}" \
                -e MailSettings__UserName="${MailSettings__UserName}" \
                -e MailSettings__PassWord="${MailSettings__PassWord}" \
                -e FIREBASE_API_KEY="${FIREBASE_API_KEY}" \
                -e FIREBASE_AUTH_EMAIL="${FIREBASE_AUTH_EMAIL}" \
                -e FIREBASE_AUTH_PASSWORD="${FIREBASE_AUTH_PASSWORD}" \
                -e FIREBASE_BUCKET="${FIREBASE_BUCKET}" \
                -d --name smartfarmapi -p 84:8080 -p 86:8081 chalsfptu/smartfarmapi'''
        }
    }
}

    }

    post {
        always {
            cleanWs()
        }
    }
}