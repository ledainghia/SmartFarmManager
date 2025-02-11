pipeline {
    agent any

    stages {
        stage('Build Docker Images') {
            steps {
                echo 'Removing old container (if exists) as'
                sh 'docker rm -f backend || true'  // Remove the container if it exists
                echo 'Building Docker Images using Docker Compose'
                sh 'docker-compose -f "docker-compose.yml" up -d --build'
            }
        }

        stage('Cleaning up') {
            steps {
                echo 'Cleaning up unused Docker resources'
                sh 'docker system prune -f'
                sh 'docker ps -a'
            }
        }
    }

    post {
        always {
            echo 'Cleaning workspace'
            cleanWs()
        }
    }
}
