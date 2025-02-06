pipeline {
    agent any

    stages {
        stage('Build Docker Images') {
            steps {
                echo 'Building Docker Images using Docker Compose'
                sh 'docker-compose -f "docker-compose.yml" up -d --build backend'
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
